using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Homeworks.Network_application_development
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPAddress[] addresses = Dns.GetHostAddresses("yandex.ru");
            Console.WriteLine("IP-addresses of site yandex.ru:");
            foreach (var address in addresses)
            {
                Console.WriteLine(address.ToString());
            }
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(addresses, 80);
            Console.WriteLine($"Connecting to address: {(socket.RemoteEndPoint as IPEndPoint)?.Address}");
        }
    }
}
