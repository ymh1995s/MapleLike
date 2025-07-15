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
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Session
{
    public class ClientSession : PacketSession
    {
        public string AccountDbId { get; private set; }
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

            // AccountDbId 메모리에 기억
            AccountDbId = loginPacket.DBId;

            using (AppDbContext db = new AppDbContext())
            {
                UserDb findAccount = db.Users.Where(a => a.UserDbId == loginPacket.DBId).FirstOrDefault();

                if (findAccount != null)
                {
                    // DB의 데이터로부터 캐릭터 생성

                    // TODO 기존의 데이터 대입하기

                    // 생성 및 입장
                    LoadOrCreatePlayer((ClassType)findAccount.ClassType, AccountDbId);
                }
                else
                {
                    // 캐릭터 생성창으로 유도
                    S_Login loginOk = new S_Login() { IsOk = 1 };
                    Send(loginOk);
                }
            }
        }

        public void SavePlayerInfoToDb(PlayerInfo playerInfo)
        {
            using (AppDbContext db = new AppDbContext())
            {
                UserDb findAccount = db.Users.Where(a => a.UserDbId == playerInfo.DbId).FirstOrDefault();

                if (findAccount == null)
                {
                    var newUser = new UserDb
                    {
                        UserDbId = playerInfo.DbId,
                        Level = playerInfo.StatInfo.Level,
                        ClassType = (int)playerInfo.StatInfo.ClassType,
                        MapNo = playerInfo.MapNo,
                        Exp = playerInfo.StatInfo.CurrentExp,
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
                        Inventory = new List<InventoryDb>() // 초기값
                    };
                    db.Users.Add(newUser);
                }
                else
                {
                    // 기존 유저 정보 수정
                    findAccount.Level = playerInfo.StatInfo.Level;
                    findAccount.ClassType = (int)playerInfo.StatInfo.ClassType;
                    findAccount.MapNo = playerInfo.MapNo;
                    findAccount.Exp = playerInfo.StatInfo.CurrentExp;
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
                }
                db.SaveChanges();
            }
        }

        // 기존에 있는 유저면 기존의 값을,
        // 그렇지 않으면 직업별로 기본 스텟을 부여한다.
        public void LoadPlayerInfoToDb(ClassType classType, string DbId)
        {
            using (AppDbContext db = new AppDbContext())
            {
                UserDb findAccount = db.Users.Where(a => a.UserDbId == DbId).FirstOrDefault();

                if (findAccount != null)
                {
                    // MyPlayer.Info.PlayerId = findAccount.PlayerId?; // ID 는 게임 내 발급으로, DB가 관리하지 않음
                    MyPlayer.Info.DbId = findAccount.UserDbId;
                    // MyPlayer.Info.Name = findAccount.name; // name 는 게임 내 발급으로, DB가 관리하지 않음
                    MyPlayer.Info.MapNo = findAccount.MapNo;
                    MyPlayer.Info.StatInfo.Level = findAccount.Level;
                    MyPlayer.Info.StatInfo.ClassType = (ClassType)findAccount.ClassType;
                    MyPlayer.Info.StatInfo.CurrentHp = findAccount.CurrentHp;
                    MyPlayer.Info.StatInfo.MaxHp = findAccount.MaxHp;
                    MyPlayer.Info.StatInfo.CurrentMp = findAccount.CurrentMp;
                    MyPlayer.Info.StatInfo.MaxMp = findAccount.MaxMp;
                    MyPlayer.Info.StatInfo.AttackPower = findAccount.AttackPower;
                    MyPlayer.Info.StatInfo.MagicPower = findAccount.MagicPower;
                    MyPlayer.Info.StatInfo.Defense = findAccount.Defense;
                    MyPlayer.Info.StatInfo.Speed = findAccount.Speed;
                    MyPlayer.Info.StatInfo.Jump = findAccount.Jump;
                    MyPlayer.Info.StatInfo.CurrentExp = findAccount.Exp;
                    MyPlayer.Info.StatInfo.MaxExp = findAccount.MaxExp;

                    MyPlayer.Info.Gold = findAccount.Gold;
                }
                else
                {
                    MyPlayer.Info.StatInfo = initStatInfo.Clone();
                    switch (classType)
                    {
                        case ClassType.Warrior:
                            MyPlayer.Info.StatInfo.MaxHp = (int)(MyPlayer.Info.StatInfo.MaxHp * 4f);
                            MyPlayer.Info.StatInfo.MaxMp = (int)(MyPlayer.Info.StatInfo.MaxMp * 1f);
                            break;
                        case ClassType.Magician:
                            MyPlayer.Info.StatInfo.MaxHp = (int)(MyPlayer.Info.StatInfo.MaxHp * 0.75f);
                            MyPlayer.Info.StatInfo.MaxMp = (int)(MyPlayer.Info.StatInfo.MaxMp * 3.5f);
                            break;
                        case ClassType.Archer:
                            MyPlayer.Info.StatInfo.MaxHp = (int)(MyPlayer.Info.StatInfo.MaxHp * 0.9f);
                            MyPlayer.Info.StatInfo.MaxMp = (int)(MyPlayer.Info.StatInfo.MaxMp * 2.5f);
                            break;
                    }

                    MyPlayer.Info.StatInfo.CurrentHp = MyPlayer.Info.StatInfo.MaxHp;
                    MyPlayer.Info.StatInfo.CurrentMp = MyPlayer.Info.StatInfo.MaxMp;
                }
            }
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
                MyPlayer.Info.Gold = -1; // 플레이어쪽 초기화 회피용 꼼수
                MyPlayer.Session = this;
            }
            
            // MyPlayer에 필요한 정보를 채움 => 패킷으로 전송(DB 데이터 로드)
            LoadPlayerInfoToDb(classType, DbId);

            if (classType == ClassType.Cnone || !Enum.IsDefined(typeof(ClassType), classType))
            {
                Console.WriteLine(" 허용되지 않은 클래스 타입. 클라이언트 오류 ");
                return;
            }

            MyPlayer.Info.StatInfo.ClassType = classType;

            GameRoom room = RoomManager.Instance.Find(MyPlayer.Info.MapNo);
            if (room == null)
            {
                Console.WriteLine($"{(int)MapName.Tutorial}번 방이 존재하지 않습니다. 클라이언트 접속 종료");
                Disconnect();
                return;
            }

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
