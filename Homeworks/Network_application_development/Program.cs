using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Homeworks.Network_application_development
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 12345);
            server.Start();

            using (TcpClient client = server.AcceptTcpClient())
            {
                Console.WriteLine("Connected");
                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());

                string? s = reader.ReadLine();
                Console.WriteLine(s);

                string r = new string(s?.Reverse().ToArray());
                writer.WriteLine(r);
                writer.Flush();
            }
        }
    }
}
