using ServerContents.Room;
using ServerContents.Session;
using ServerCore;
using System.Net;

namespace ServerContents
{
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // TEMP. 순서대로 튜토리얼, 사냥터, 보스
            GameRoom room1 = RoomManager.Instance.Add(); 
            GameRoom room2 = RoomManager.Instance.Add();
            GameRoom room3 = RoomManager.Instance.Add();

            Console.WriteLine("Server Start!");
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // AWS 전용 하드 코딩
            string ipAddressString = "127.0.0.1";
            IPAddress ipAddr = IPAddress.Parse(ipAddressString);
            //IPAddress ipAddr = ipHost.AddressList[0]; 
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // Func.Invoke() (Listener의 _sessionFactory.Invoke();) 에 의해 SessionManager.Instance.Generate() 가 N번 생성됨
            // Delegate인 SessionManager.Instance.Generate()는 여기서 당장 실행 되진 않고 인자로써 넘겨준다.
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Console.WriteLine($"This Server IP is {endPoint.Address.ToString()}");
            Console.WriteLine("Server Listening...");

            while (true)
            {
                // TODO. 방을 나누고 각각의 스레드로 배분한다.
                room1.Push(room1.Flush);
                room2.Push(room2.Flush);
                room3.Push(room3.Flush);

                Thread.Sleep(1);
            }
        }
    }
}
