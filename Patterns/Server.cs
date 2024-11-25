using Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Patterns
{
    public enum TypeSend
    {
        ToOne,
        ToAll,
        Default
    }
    public class Server
    {
        public string Name { get => "Server-1"; }
        public Dictionary<string, IPEndPoint> Users { get; set; }
        private readonly UdpClient _udpClient;
        private IPEndPoint _iPEndPoint;
        private Manager _manager;
        public Server()
        {
            _udpClient = new UdpClient(12345);
            _iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            _manager = new Manager(this);
        }
        public Message Listen()
        {
            byte[] buffer = _udpClient.Receive(ref _iPEndPoint);
            string messageText = Encoding.UTF8.GetString(buffer);
            return Message.DeserializeMessageFromJson(messageText);
        }
        public void Send(TypeSend typeSend, Message message)
        {
            byte[] reply = Encoding.UTF8.GetBytes(message.SerializeMessageToJson());
            switch (typeSend)
            {
                case TypeSend.ToAll:
                    foreach (var ip in Users.Values)
                        _udpClient.Send(reply, reply.Length, _iPEndPoint);
                    break;
                case TypeSend.ToOne:
                    if(Users.TryGetValue(message.NicknameTo, out IPEndPoint ep))
                        _udpClient.Send(reply, reply.Length, _iPEndPoint);
                    break;
                case TypeSend.Default:
                    break;
                default:
                    break;
            }
        }
        public void Run()
        {
            Console.WriteLine("Waiting for connection...");
            while (true)
            {
                var mes = Listen();
                var typeSend = _manager.Execute(mes, _iPEndPoint);
                ThreadPool.QueueUserWorkItem(obj =>
                {
                    Send(typeSend, mes);
                });
            }
        }
    }
}
