using Network;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    internal class ClientCode
    {
        public static void Client(string nickFrom = "Aleksey", string ip = "127.0.0.1")
        {
            Message msg = new Message() { DateTime = DateTime.Now, NicknameFrom = nickFrom, NicknameTo = "Server" };
            using (UdpClient client = new UdpClient())
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), 12345);
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
                    Console.WriteLine($"Отправлено {buffer.Length} байт");
                    if (msg.Text.Equals("Exit"))
                    {
                        Console.WriteLine("Чат завершён.");
                        break;
                    }
                    string answer = Encoding.UTF8.GetString(client.Receive(ref localEndPoint));
                    Console.WriteLine(answer ?? "Что-то пошло не так!");
                }
            }
        }
    }
}
