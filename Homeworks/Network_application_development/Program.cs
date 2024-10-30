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
                while (count < 10)
                {
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.None, 0);
                    var sf = new SocketFlags();
                    int c = server.ReceiveMessageFrom(buffer, ref sf, ref remoteEndPoint, out IPPacketInformation info);
                    if (c == 1)
                    {
                        var ep = remoteEndPoint as IPEndPoint;
                        Console.WriteLine($"Получено {info.Interface}, flags = {sf} от {ep.Address}:{ep.Port} значение [{buffer[0]}]");
                    }
                    count += c;
                }
                Console.WriteLine("\n Прочли 200 байт");
            }
        }
    }
}
