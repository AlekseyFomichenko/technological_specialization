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
                Console.Clear();
                while (true)
                {
                    do
                    {
                        Console.Write("Введите сообщение: ");
                        message = Console.ReadLine()!;
                    }
                    while (string.IsNullOrWhiteSpace(message));

                    msg.Text = message;
                    string messageFromJson = msg.SerializeMessageToJson();
                    byte[] buffer = Encoding.UTF8.GetBytes(messageFromJson);
                    client.Send(buffer, buffer.Length, localEndPoint);
                    string answer = Encoding.UTF8.GetString(client.Receive(ref localEndPoint));
                    Console.WriteLine(answer ?? "Что-то пошло не так!");
                }
            }
        }
    }
}
