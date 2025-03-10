using ServerContents.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Room
{
    // 유틸 관련 기능
    // 개략적 성능 확인 기능
    // 기타 기능
    public partial class GameRoom : JobSerializer
    {
        #region 패킷 송수신 개수 개략적 확인용
        public int recvPacketCount = 0;
        public int sendPacketCount = 0;
        public void RecvPacketPlus()
        {
            recvPacketCount++;
        }

        public void SendPacketPlus()
        {
            sendPacketCount++;
        }

        private async void PrintProceecPacket()
        {
            while (true)
            {
                Console.WriteLine($"{RoomId}번 방에서 총{recvPacketCount + sendPacketCount}, recv : {recvPacketCount}개 / send : {sendPacketCount}개을 1초에 처리");

                recvPacketCount = 0;
                sendPacketCount = 0;

                await Task.Delay(1000); // 1초 대기 (비동기적으로 실행)
            }
        }

        #endregion 패킷 송수신 개수 개략적 확인용
    }
}
