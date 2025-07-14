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

            //MyPlayer = ObjectManager.Instance.Add<Player>();
            //{
            //    MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
            //    MyPlayer.Session = this;
            //}

            //Console.WriteLine($"{endPoint} Object Added in Dic, Send Enter packet To Server...");

            //GameRoom room = RoomManager.Instance.Find(1);
            //if(room ==null)
            //{
            //    Console.WriteLine( $"N번 방이 존재하지 않습니다. 클라이언트 접속 종료" );
            //    Disconnect();
            //    return;
            //}

            //room.Push(room.PlayerEnterGame, MyPlayer, 0);
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
                    LoadOrCreatePlayer((ClassType)findAccount.Job, AccountDbId);
                }
                else
                {
                    // 캐릭터 생성창으로 유도
                    S_Login loginOk = new S_Login() { IsOk = 1 };
                    Send(loginOk);
                }
            }
        }

        public void LoadOrCreatePlayer(ClassType classType, string DbId)
        {
            using (AppDbContext db = new AppDbContext())
            {
                UserDb findAccount = db.Users.Where(a => a.UserDbId == DbId).FirstOrDefault();

                if (findAccount == null)
                {
                    var newUser = new UserDb
                    {
                        UserDbId = DbId,

                        Level = 1,
                        Job = 0,              // 예: 0 = 무직
                        MapNo = 1001,         // 시작 맵 번호

                        Exp = 0,
                        MaxExp = 100,

                        CurrentHp = 100,
                        MaxHp = 100,

                        CurrentMp = 50,
                        MaxMp = 50,

                        Attack = 10,
                        Defense = 5,

                        MoveSpeed = 5.0f,
                        JumpPower = 3.5f,

                        Gold = 100,

                        Inventory = new List<InventoryDb>() // 비어 있는 인벤토리 초기화
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();
                }
            }

            if (MyPlayer != null)
            {
                Console.WriteLine("이미 캐릭터를 생성한 클라이언트 입니다");
                return;
            }
            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
                MyPlayer.Session = this;
            }

            if (classType == ClassType.Cnone || !Enum.IsDefined(typeof(ClassType), classType))
            {
                Console.WriteLine(" 허용되지 않은 클래스 타입. 클라이언트 오류 ");
                return;
            }

            MyPlayer.Info.StatInfo.ClassType = classType;

            GameRoom room = RoomManager.Instance.Find((int)MapName.Tutorial);
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
