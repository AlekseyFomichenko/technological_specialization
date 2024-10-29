using System.Net;
using System.Net.Sockets;

namespace Homeworks.Network_application_development
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5564);
                server.Bind(iPEndPoint);
                server.Blocking = true;
                Console.WriteLine("Waiting for connecting....");
                server.Listen(100);
                Socket socket = server.Accept();
                Console.WriteLine("Connected!");
                server.Close();
            }
        }
    }
}
