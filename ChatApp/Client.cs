using ChatCommon.Models;
using ChatCommon.Abstracts;
using System.Net;

namespace ChatApp
{
    public class Client<T>
    {
        private readonly string _name;
        private readonly IMessageSourceClient<T> _messageSource;
        T remoteEndPoint;
        public Client(IMessageSourceClient<T> messageSourceClient, string name)
        {
            _name = name;
            _messageSource = messageSourceClient;
            remoteEndPoint = _messageSource.CreateEndPoint();
        }
        //public UdpClient UdpClient = new UdpClient();
        public async Task Start()
        {
            await ClientSendler();
            await ClientListener();
            //new Thread(async () => await ClientListener()).Start();
        }

        public async Task ClientSendler()
        {
            Register(remoteEndPoint);
            while (true)
            {
                try
                {
                    Console.WriteLine("Введите имя получателя:");
                    string? nameTo = Console.ReadLine();
                    Console.WriteLine("Введите текст сообщения: ");
                    string? messageText = Console.ReadLine();
                    var message = new NetMessage { Command = Command.Message, NickNameFrom = _name, NickNameTo = nameTo, Text = messageText };
                    await _messageSource.SendAsync(message, remoteEndPoint);
                    Console.WriteLine("Сообщение отправлено");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Что-то пошло не так: " + ex.Message);
                }
            }
        }

        public async Task ClientListener()
        {
            while (true)
            {
                try
                {
                    var messageReceive = _messageSource.Receive(ref remoteEndPoint);
                    Console.WriteLine($"Получено сообщение от: {messageReceive.NickNameFrom}");
                    Console.WriteLine(messageReceive.Text);
                    await Confirm(messageReceive, remoteEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении сообщения " + ex.Message);
                }
            }
        }

        private async Task Confirm(NetMessage messageReceive, T remoteEndPoint)
        {
            messageReceive.Command = Command.Confirmation;
            await _messageSource.SendAsync(messageReceive, remoteEndPoint);
        }
        public void Register(T iPEndPoint)
        {
            T ep = _messageSource.CreateEndPoint();
            var message = new NetMessage
            {
                NickNameFrom = _name,
                NickNameTo = null,
                Text = null,
                Command = Command.Register,
                EndPoint = iPEndPoint as IPEndPoint
            };
            _messageSource.SendAsync(message, ep);
        }
    }
}
