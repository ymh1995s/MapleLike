using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ServerContents.DB;
using ServerContents.Object;
using ServerContents.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Session
{
    public class ClientSession : PacketSession
    {
        public string DbId_Forward; // 기본적으로 MyPlayer 객체 생성 전 '임시'로 DBId를 저장하는 프로퍼티
        public Player MyPlayer { get; set; }
        public int SessionId { get; set; }

        private PlayerStatInfo initStatInfo = new PlayerStatInfo()
        {
            // 캐릭터 초기화 스탯 데이터
            // 아무것도 설정되지 않은 플레이어의 기본 스탯이다.
            Level = 1,
            ClassType = ClassType.Cnone,
            CurrentHp = 100,
            MaxHp = 100,
            CurrentMp = 150,
            MaxMp = 150,
            AttackPower = 10,
            MagicPower = 10,
            Defense = 1,
            Speed = 3,
            Jump = 10,
            CurrentExp = 0,
            MaxExp = 100,
            STR = 4,
            DEX = 4,
            INT = 4,
            LUK = 4
        };

        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4]; // 4 : 헤더(2 바이트) + 패킷종류(2 바이트)
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort)); // ushort : 헤더(2 바이트)
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));      // ushort : 패킷종류(2 바이트)
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);                                // size : 헤더 패킷을 제외한 패킷 데이터 크기 
            Send(new ArraySegment<byte>(sendBuffer));

            //Console.WriteLine($"Send {msgName} message to client");
        }

        // 패킷 모아보내기 - 전송을 위해 IMeesage 타입을 ArraySegment 타입으로 변환
        // 로직상 스레드세이프
        public void Send(List<IMessage> packetList)
        {
            if (packetList.Count == 0)
                return;

            List<ArraySegment<byte>> _packetList = new List<ArraySegment<byte>>();

            foreach (IMessage packet in packetList)
            {
                string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
                MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
                ushort size = (ushort)packet.CalculateSize();
                byte[] sendBuffer = new byte[size + 4];                                                  // 4 : 헤더(2 바이트) + 패킷종류(2 바이트)
                Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort)); // ushort : 헤더(2 바이트)
                Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));      // ushort : 패킷종류(2 바이트)
                Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);                                // size : 헤더 패킷을 제외한 패킷 데이터 크기 
                _packetList.Add(sendBuffer);
            }
            Send(_packetList);
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine( $"client {endPoint} is connected to the server. Here is server" );

            S_Connected connectedPkt = new S_Connected();
            Send(connectedPkt);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"client {endPoint} is disconnected from the server. Here is server");

            if(MyPlayer != null)
            {                
                GameRoom room = RoomManager.Instance.Find(MyPlayer.Room.RoomId);
                if (room == null)
                {
                    Console.WriteLine( $"\"{MyPlayer.Room.RoomId}\"룸을 찾지 못했습니다. 로직 에러 (고의적 크래쉬..)" );
                }
                room.Push(room.LeavePlayer, MyPlayer.Info.PlayerId);
            }

            MyPlayer = null;
            SessionManager.Instance.Remove(this);
        }

        public void HandleLogin(C_Login loginPacket)
        {
            // TODO : 레디스에서 추가 검증해서 로그인 성공 / 실패 2중 검사

            DbId_Forward = loginPacket.DBId;

            // 기존 캐릭터 로드 Or 캐릭터 생성창 유도
            using (AppDbContext db = new AppDbContext())
            {
                UserDb findAccount = db.Users.Where(a => a.DbId == loginPacket.DBId).FirstOrDefault();

                if (findAccount != null)
                {
                    // 기존 캐릭터 로드(From DB) 및 입장
                    LoadOrCreatePlayer((ClassType)findAccount.ClassType, DbId_Forward);
                }
                else
                {
                    // 캐릭터 생성창으로 유도
                    S_Login loginOk = new S_Login() { IsOk = 1 };
                    Send(loginOk);
                }
            }
        }

        public void SavePlayerInfo(PlayerInfo playerInfo)
        {
            // 사망했을 시는 예외적으로 HP MP 조정
            if (playerInfo.StatInfo.CurrentHp == 0)
            {
                playerInfo.StatInfo.CurrentHp = 50;
                playerInfo.StatInfo.CurrentMp = 50;
            }

            SavePlayerInfoToMem(playerInfo);
            SavePlayerInfoToDb(playerInfo);
        }

        // 1개의 아이템 저장 요청이 왔을 때
        public void SaveItemInfo(ItemInfo itemInfo)
        {
            SaveItemInfoToMem(itemInfo);

            using (AppDbContext db = new AppDbContext())
            {
                // 1. DbId가 있는 유저 테이블을 찾고
                // 2. 인벤토리까지 함께 로드함
                UserDb findAccount = db.Users
                    .Include(u => u.Inventory)
                    .FirstOrDefault(a => a.DbId == MyPlayer.Info.DbId);

                SaveItemInfoToDb(db, findAccount, itemInfo);

                db.SaveChanges();
            }
        }


        public void SavePlayerInfoToDb(PlayerInfo playerInfo)
        {
            using (AppDbContext db = new AppDbContext())
            {
                // 1. DbId가 있는 유저 테이블을 찾고
                // 2. 인벤토리까지 함께 로드함
                UserDb findAccount = db.Users
                    .Include(u => u.Inventory)
                    .FirstOrDefault(a => a.DbId == MyPlayer.Info.DbId);

                if (findAccount == null)
                {
                    var newUser = new UserDb
                    {
                        DbId = playerInfo.DbId,
                        Level = playerInfo.StatInfo.Level,
                        ClassType = playerInfo.StatInfo.ClassType,
                        MapNo = playerInfo.MapNo,
                        CurrentExp = playerInfo.StatInfo.CurrentExp,
                        MaxExp = playerInfo.StatInfo.MaxExp,
                        CurrentHp = playerInfo.StatInfo.CurrentHp,
                        MaxHp = playerInfo.StatInfo.MaxHp,
                        CurrentMp = playerInfo.StatInfo.CurrentMp,
                        MaxMp = playerInfo.StatInfo.MaxMp,
                        AttackPower = playerInfo.StatInfo.AttackPower,
                        MagicPower = playerInfo.StatInfo.MagicPower,
                        Defense = playerInfo.StatInfo.Defense,
                        Speed = playerInfo.StatInfo.Speed,
                        Jump = playerInfo.StatInfo.Jump,
                        STR = playerInfo.StatInfo.STR,
                        DEX = playerInfo.StatInfo.DEX,
                        INT = playerInfo.StatInfo.INT,
                        LUK = playerInfo.StatInfo.LUK,
                        Gold = playerInfo.Gold,
                        Inventory = new List<InventoryDb>()
                    };

                    foreach (var item in playerInfo.Inventory.ItemInfo)
                    {
                        newUser.Inventory.Add(new InventoryDb
                        {
                            ItemDbId = item.ItemId,
                            Count = item.ItemCount,
                            MaxCount = 9999, // 필요하면 설정
                            IsEquipped = false, // 기본값
                            UserDbId = playerInfo.DbId
                        });
                    }

                    db.Users.Add(newUser);
                }
                else
                {
                    // 기존 유저 정보 수정
                    findAccount.DbId = playerInfo.DbId;
                    findAccount.Level = playerInfo.StatInfo.Level;
                    findAccount.ClassType = playerInfo.StatInfo.ClassType;
                    findAccount.MapNo = playerInfo.MapNo;
                    findAccount.CurrentExp = playerInfo.StatInfo.CurrentExp;
                    findAccount.MaxExp = playerInfo.StatInfo.MaxExp;
                    findAccount.CurrentHp = playerInfo.StatInfo.CurrentHp;
                    findAccount.MaxHp = playerInfo.StatInfo.MaxHp;
                    findAccount.CurrentMp = playerInfo.StatInfo.CurrentMp;
                    findAccount.MaxMp = playerInfo.StatInfo.MaxMp;
                    findAccount.AttackPower = playerInfo.StatInfo.AttackPower;
                    findAccount.MagicPower = playerInfo.StatInfo.MagicPower;
                    findAccount.Defense = playerInfo.StatInfo.Defense;
                    findAccount.Speed = playerInfo.StatInfo.Speed;
                    findAccount.Jump = playerInfo.StatInfo.Jump;
                    findAccount.STR = playerInfo.StatInfo.STR;
                    findAccount.DEX = playerInfo.StatInfo.DEX;
                    findAccount.INT = playerInfo.StatInfo.INT;
                    findAccount.LUK = playerInfo.StatInfo.LUK;
                    findAccount.Gold = playerInfo.Gold;

                    foreach (var item in playerInfo.Inventory.ItemInfo)
                    {
                        SaveItemInfoToDb(db, findAccount, item);
                    }
                }
                db.SaveChanges();
            }
        }

        public void SavePlayerInfoToMem(PlayerInfo playerInfo)
        {
            // 기존 유저 정보 수정
            MyPlayer.Info.DbId = playerInfo.DbId;
            MyPlayer.Stat.Level = playerInfo.StatInfo.Level;
            MyPlayer.Stat.ClassType = playerInfo.StatInfo.ClassType;
            MyPlayer.Info.MapNo = playerInfo.MapNo;
            MyPlayer.Stat.CurrentExp = playerInfo.StatInfo.CurrentExp;
            MyPlayer.Stat.MaxExp = playerInfo.StatInfo.MaxExp;
            MyPlayer.Stat.CurrentHp = playerInfo.StatInfo.CurrentHp;
            MyPlayer.Stat.MaxHp = playerInfo.StatInfo.MaxHp;
            MyPlayer.Stat.CurrentMp = playerInfo.StatInfo.CurrentMp;
            MyPlayer.Stat.MaxMp = playerInfo.StatInfo.MaxMp;
            MyPlayer.Stat.AttackPower = playerInfo.StatInfo.AttackPower;
            MyPlayer.Stat.MagicPower = playerInfo.StatInfo.MagicPower;
            MyPlayer.Stat.Defense = playerInfo.StatInfo.Defense;
            MyPlayer.Stat.Speed = playerInfo.StatInfo.Speed;
            MyPlayer.Stat.Jump = playerInfo.StatInfo.Jump;
            MyPlayer.Stat.STR = playerInfo.StatInfo.STR;
            MyPlayer.Stat.DEX = playerInfo.StatInfo.DEX;
            MyPlayer.Stat.INT = playerInfo.StatInfo.INT;
            MyPlayer.Stat.LUK = playerInfo.StatInfo.LUK;
            MyPlayer.Info.Gold = playerInfo.Gold;

            foreach (var item in playerInfo.Inventory.ItemInfo)
            {
                SaveItemInfoToMem(item);
            }
        }

        public void SaveItemInfoToMem(ItemInfo itemInfo)
        {
            var item = MyPlayer.Info.Inventory.ItemInfo
                .FirstOrDefault(i => i.ItemType == itemInfo.ItemType);

            if (item != null)
            {
                item.ItemCount = itemInfo.ItemCount;
            }
            else
            {                                  
                // 없으면 새로 추가
                MyPlayer.Info.Inventory.ItemInfo.Add(itemInfo);
            }
        }

        public void SaveItemInfoToDb(AppDbContext db, UserDb user, ItemInfo itemInfo)
        {
            if (user == null)
                return;

            // 동일한 아이템이 이미 있는지 검사
            var existingItem = user.Inventory
                .FirstOrDefault(i => i.ItemDbId == itemInfo.ItemId);

            if (existingItem != null)
            {
                // 이미 있으면 수량 재설정
                existingItem.Count = itemInfo.ItemCount;
            }
            else
            {
                // 없으면 새 인벤토리 엔트리 생성
                InventoryDb newInventoryItem = new InventoryDb
                {
                    UserDbId = user.DbId,
                    ItemDbId = itemInfo.ItemId,
                    Count = itemInfo.ItemCount,
                    MaxCount = 99, // 필요에 따라 설정
                    IsEquipped = false
                };


                db.Inventories.Add(newInventoryItem);
            }
        }

        // 기존에 있는 유저면 DB 값을, 최초 유저는 그렇지 않으면 직업별로 기본 값을 부여
        public Player LoadPlayerInfoFromDb(ClassType classType, string DbId)
        {
            Player retPlayer = MyPlayer;
            using (AppDbContext db = new AppDbContext())
            {
                // 1. DbId가 있는 유저 테이블을 찾고
                // 2. 인벤토리까지 함께 로드함
                // 3. 필요 시 item 테이블도 로드? => 보류
                UserDb findAccount = db.Users
                    .Include(u => u.Inventory)         // User의 Inventory까지 미리 로드
                    //.ThenInclude(inv => inv.Item)     // 여긴 필요한가?
                    .FirstOrDefault(a => a.DbId == DbId);

                if (findAccount != null)
                {
                    // 기존 사용자 => DB에서 가져오기
                    // MyPlayer.Info.PlayerId = findAccount.PlayerId?; // ID 는 게임 내 발급으로, DB가 관리하지 않음
                    retPlayer.Info.DbId = findAccount.DbId;
                    // MyPlayer.Info.Name = findAccount.name; // name 는 게임 내 발급으로, DB가 관리하지 않음
                    retPlayer.Info.MapNo = findAccount.MapNo;
                    retPlayer.Info.StatInfo.Level = findAccount.Level;
                    retPlayer.Info.StatInfo.ClassType = (ClassType)findAccount.ClassType;
                    retPlayer.Info.StatInfo.CurrentHp = findAccount.CurrentHp;
                    retPlayer.Info.StatInfo.MaxHp = findAccount.MaxHp;
                    retPlayer.Info.StatInfo.CurrentMp = findAccount.CurrentMp;
                    retPlayer.Info.StatInfo.MaxMp = findAccount.MaxMp;
                    retPlayer.Info.StatInfo.AttackPower = findAccount.AttackPower;
                    retPlayer.Info.StatInfo.MagicPower = findAccount.MagicPower;
                    retPlayer.Info.StatInfo.Defense = findAccount.Defense;
                    retPlayer.Info.StatInfo.Speed = findAccount.Speed;
                    retPlayer.Info.StatInfo.Jump = findAccount.Jump;
                    retPlayer.Info.StatInfo.CurrentExp = findAccount.CurrentExp;
                    retPlayer.Info.StatInfo.MaxExp = findAccount.MaxExp;
                    retPlayer.Info.StatInfo.STR = findAccount.STR;
                    retPlayer.Info.StatInfo.DEX = findAccount.DEX;
                    retPlayer.Info.StatInfo.INT = findAccount.INT;
                    retPlayer.Info.StatInfo.LUK = findAccount.LUK;
                    retPlayer.Info.Gold = findAccount.Gold;

                    retPlayer.Info.Inventory = new Inventory();

                    foreach (var dbItem in findAccount.Inventory)  // InventoryDb 리스트 순회
                    {
                        retPlayer.Info.Inventory.ItemInfo.Add(new ItemInfo
                        {
                            ItemId = dbItem.ItemDbId,   // 아이템 고유 ID
                            // ItemType = dbItem.Item.ItemType,  // 아이템 종류 (Item 테이블에서 불러온 값) => 보류
                            ItemCount = dbItem.Count      // 소지 개수
                        });
                    }
                }
                else
                {
                    // 새로운 사용자 => 기본 값 부여
                    retPlayer.Info.StatInfo = initStatInfo.Clone();
                    switch (classType)
                    {
                        case ClassType.Warrior:
                            retPlayer.Info.StatInfo.MaxHp = (int)(retPlayer.Info.StatInfo.MaxHp * 4f);
                            retPlayer.Info.StatInfo.MaxMp = (int)(retPlayer.Info.StatInfo.MaxMp * 1f);
                            break;
                        case ClassType.Magician:
                            retPlayer.Info.StatInfo.MaxHp = (int)(retPlayer.Info.StatInfo.MaxHp * 0.75f);
                            retPlayer.Info.StatInfo.MaxMp = (int)(retPlayer.Info.StatInfo.MaxMp * 3.5f);
                            break;
                        case ClassType.Archer:
                            retPlayer.Info.StatInfo.MaxHp = (int)(retPlayer.Info.StatInfo.MaxHp * 0.9f);
                            retPlayer.Info.StatInfo.MaxMp = (int)(retPlayer.Info.StatInfo.MaxMp * 2.5f);
                            break;
                    }

                    retPlayer.Info.StatInfo.CurrentHp = retPlayer.Info.StatInfo.MaxHp;
                    retPlayer.Info.StatInfo.CurrentMp = retPlayer.Info.StatInfo.MaxMp;

                    // 기타 최초 캐릭터 생성 시 초기화 하는 값들
                    retPlayer.Info.StatInfo.ClassType = classType;
                    retPlayer.Info.Gold = 1000;
                    
                    if (retPlayer.Info.Inventory == null) retPlayer.Info.Inventory = new Inventory();
                    retPlayer.Info.Inventory.ItemInfo.Add(new ItemInfo { ItemId = (int)ItemType.Hppotion1, ItemCount = 99 });
                    retPlayer.Info.Inventory.ItemInfo.Add(new ItemInfo { ItemId = (int)ItemType.Mppotion1, ItemCount = 99 });
                    // TODO 여기에 기본 아이템과 포션 넣어줘야겠지?
                }
            }
            return retPlayer;
        }

        public void LoadOrCreatePlayer(ClassType classType, string DbId)
        {
            if (MyPlayer != null)
            {
                Console.WriteLine("이미 캐릭터를 생성한 클라이언트 입니다");
                return;
            }

            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
                MyPlayer.Info.DbId = DbId;
                MyPlayer.Info.StatInfo.ClassType = classType;
                MyPlayer.Session = this;
            }

            // MyPlayer에 필요한 정보를 채움 => 패킷으로 전송(DB 데이터 로드)
            MyPlayer = LoadPlayerInfoFromDb(classType, DbId);

            if (classType == ClassType.Cnone || !Enum.IsDefined(typeof(ClassType), classType))
            {
                Console.WriteLine(" 허용되지 않은 클래스 타입. 클라이언트 오류 ");
                return;
            }

            GameRoom room = RoomManager.Instance.Find(MyPlayer.Info.MapNo);
            if (room == null)
            {
                Console.WriteLine($"{(int)MapName.Tutorial}번 방이 존재하지 않습니다. 클라이언트 접속 종료");
                Disconnect();
                return;
            }

            SavePlayerInfoToDb(MyPlayer.Info);
            room.Push(room.PlayerEnterGame, MyPlayer, 0);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            //Console.WriteLine("Receive message from client");
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine("Send message to client");
        }
    }
}
