using System.Net;
using System.Net.Sockets;

namespace Clients
{
    public class Program
    {
        public static void Send(byte[] buffer, Socket socket)
        {
            for (int i = 0; i < 100; i++)
            {
                int count = socket.Send(buffer);
            }
            socket.Close();
        }
        static void Main(string[] args)
        {
            Socket client1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket client2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint iPEndPointClient1 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7744);
            IPEndPoint iPEndPointClient2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7755);

            client1.Bind(iPEndPointClient1);
            client2.Bind(iPEndPointClient2);

            client1.Connect("127.0.0.1", 1234);
            client2.Connect("127.0.0.1", 1234);

            (new Thread(() => Send(new byte[] { 1 }, client1))).Start();
            (new Thread(() => Send(new byte[] { 2 }, client2))).Start();
        }
    }
}
