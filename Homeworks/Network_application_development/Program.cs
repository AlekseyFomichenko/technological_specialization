using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Homeworks.Network_application_development
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1234);
                server.Bind(localEndPoint);
                byte[] buffer = new byte[1];
                int count = 0;
                while (count < 200)
                {
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.None, 0);
                    int c = server.ReceiveFrom(buffer, ref remoteEndPoint);
                    if (c == 1)
                    {
                        if((remoteEndPoint as IPEndPoint)?.Port == 7755)
                            Console.Write(buffer[0]);
                    }
                    count += c;
                }
                Console.WriteLine("\n Прочли 200 байт");
            }
        }
    }
}
