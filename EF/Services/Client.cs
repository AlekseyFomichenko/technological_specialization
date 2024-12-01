using EFSeminar.Abstracts;
using EFSeminar.Models;
using System.Net;

namespace EFSeminar.Services
{
    public class Client
    {
        private readonly string _name;
        private readonly IMessageSource _messageSource;
        IPEndPoint remoteEndPoint;
        public Client(string name, string address, int port)
        {
            _name = name;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            _messageSource = new UdpMessageSource();
        }
        public async Task Start()
        {
            await ClientListener();
            await ClientSendler();
        }

        async Task ClientSendler()
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

        async Task ClientListener()
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

        async Task Confirm(NetMessage messageReceive, IPEndPoint remoteEndPoint)
        {
            messageReceive.Command = Command.Confirmation;
            await _messageSource.SendAsync(messageReceive, remoteEndPoint);
        }
        void Register(IPEndPoint iPEndPoint)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            var message = new NetMessage
            {
                NickNameFrom = _name,
                NickNameTo = null,
                Text = null,
                Command = Command.Register,
                EndPoint = iPEndPoint
            };
            _messageSource.SendAsync(message, ep);
        }
    }
}
