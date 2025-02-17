using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Session
{
    class ServerSession : PacketSession
    {
        public PositionInfo dummyPosition { get; private set; } = new PositionInfo();

        Random _rand = new Random();

        public ServerSession() 
        {
            // 시작 위치를 랜덤으로 정한다.
            dummyPosition.DestinationPosX = _rand.Next(-20, 20);
            dummyPosition.DestinationPosY = _rand.Next(0, 5);
            dummyPosition.DestinationPosZ = _rand.Next(-20, 20);
        }

        public PositionInfo SetNextPos()
        {
            //dummyPosition.DestinationPosX += 0.02f;
            return dummyPosition;
        }

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
        }


        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"client {endPoint} is connected to the server. Here is client");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"client {endPoint} is disconnected from the server. Here is client");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {

        }
            //Console.WriteLine("Receive message from server");
        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine("Send message to server");
        }
    }
}
