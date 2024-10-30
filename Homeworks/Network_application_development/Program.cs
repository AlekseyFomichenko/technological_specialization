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
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 1234);
                server.Bind(iPEndPoint);
                byte[] buffer = new byte[1];
                int count = 0;
                while (count <= 200)
                {
                    int c = server.Receive(buffer);
                    if (c == 1)
                    {
                        Console.Write(buffer[0]);
                    }
                    count += c;
                }
                Console.WriteLine("\n Прочли 200 байт");
            }
        }
    }
}
