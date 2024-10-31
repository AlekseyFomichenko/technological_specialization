using Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (UdpClient server = new UdpClient(12345))
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Console.WriteLine("Waiting for connecting...");

                int count = 0;
                while (true)
                {
                    byte[] buffer = server.Receive(ref localEndPoint);
                    if (buffer == null) break;
                    string messageText = Encoding.UTF8.GetString(buffer);
                    Message? message = Message.DeserializeMessageFromJson(messageText);
                    message?.PrintMessageInfo();
                    
                }
            }
        }
    }
}
