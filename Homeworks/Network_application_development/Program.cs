using System.Net;
using System.Net.Sockets;
using System.Text;

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
                server.Blocking = false;
                server.Listen(100);
                Console.WriteLine("Waiting for connecting....");
                var task = server.AcceptAsync();
                while (!task.IsCompleted)
                {
                    Console.Write('.');
                    Thread.Sleep(1000);
                }
                Socket socket = task.Result;
                Console.WriteLine("Connected!");
                byte[] buffer = new byte[255];
                int count = socket.Receive(buffer);
                if (count > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine(message);
                }
                else Console.WriteLine("Message isn't sent");
                server.Close();
            }
        }
    }
}
