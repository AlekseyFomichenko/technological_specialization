using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Homeworks.Network_application_development
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("yandex.ru", 80);
            Console.WriteLine($"Connecting to address: {(socket.RemoteEndPoint as IPEndPoint)?.Address}");

            socket.Disconnect(true);

            var task = socket.ConnectAsync("google.ru", 80);
            task.Wait();

            Console.WriteLine($"Connecting to address: {(socket.RemoteEndPoint as IPEndPoint)?.Address}");
        }
    }
}
