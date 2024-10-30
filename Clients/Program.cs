using System.Net;
using System.Net.Sockets;

namespace Clients
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (TcpClient client = new TcpClient())
            {
                Console.WriteLine("Connecting...");
                client.Connect(IPAddress.Parse("127.0.0.1"), 12345);
                Console.WriteLine("Connected!");

                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());

                writer.WriteLine("Hi nigga");
                writer.Flush();

                string? s = reader.ReadLine();
                Console.WriteLine(s);
            }
        }
    }
}
