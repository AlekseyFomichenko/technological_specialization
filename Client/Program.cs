using Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            Message msg = new Message() { DateTime = DateTime.Now, NicknameFrom = args[0], NicknameTo = "Server"};
            using (UdpClient client = new UdpClient())
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(args[1]), 12345);
                string message;
                while (true)
                {
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Введите сообщение:");
                        message = Console.ReadLine()!;
                    }
                    while (string.IsNullOrEmpty(message));

                    msg.Text = message;
                    string messageFromJson = msg.SerializeMessageToJson();
                    byte[] buffer = Encoding.UTF8.GetBytes(messageFromJson);
                    client.Send(buffer, buffer.Length, localEndPoint);
                }
                

            }
        }
    }
}
