using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5564);
                Console.WriteLine("Connecting...");
                client.Connect(remoteEndPoint);
                Console.WriteLine("Connected!");
                byte[] buffer = Encoding.UTF8.GetBytes("Hello!");
                int count = client.Send(buffer);
                if (count == buffer.Length) Console.WriteLine("Message was sent.");
                else Console.WriteLine("Something is wrong");
            }
            Console.ReadLine();
        }
    }
}
