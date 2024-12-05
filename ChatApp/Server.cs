using ChatCommon.Abstracts;
using ChatCommon.Models;
using ChatDB;

namespace ChatApp
{
    public class Server<T>
    {
        Dictionary<string, T> clients = new Dictionary<string, T>();
        private readonly IMessageSourceServer<T> _messageSource;
        private T ep;
        
        public Server(IMessageSourceServer<T> messageSource)
        {
            _messageSource = messageSource;
            ep = _messageSource.CreateEndPoint();
        }
        bool work = true;
        public void Stop() => work = false;
        public async Task Work()
        {
            Console.WriteLine("Сервер ожидает своё сообщение");
            while (work)
            {
                try
                {
                    var message = _messageSource.Receive(ref ep);
                    Console.WriteLine(message.ToString());
                    await ProcessMessage(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        private async Task Register(NetMessage message)
        {
            Console.WriteLine($"Message Register name = {message.NickNameFrom}");
            if (clients.TryAdd(message.NickNameFrom, _messageSource.CopyEndPoint(message.EndPoint)))
            {
                using(ChatContext context = new ChatContext())
                {
                    context.Users.Add(new User() { FullName = message.NickNameFrom });
                    await context.SaveChangesAsync();
                }
            }
        }
        private async Task RelyMessage(NetMessage netMessage)
        {
            if (clients.TryGetValue(netMessage.NickNameTo, out T iPEnd))
            {
                int? id = null;
                using (ChatContext context = new ChatContext())
                {
                    var fromUser = context.Users.First(x => x.FullName == netMessage.NickNameFrom);
                    var toUser = context.Users.First(x => x.FullName == netMessage.NickNameTo);
                    var msg = new Message() { UserFrom = fromUser, UserTo = toUser, IsSent = false, Text = netMessage.Text};
                    context.Messages.Add(msg);
                    context.SaveChanges();
                    id = msg.MessageId;
                }
                netMessage.Id = id;
                await _messageSource. SendAsync(netMessage, iPEnd);
                Console.WriteLine($"Message relied, from = {netMessage.NickNameFrom}, to = {netMessage.NickNameTo}");
            }
            else Console.WriteLine("Пользователь не найден");
        }
        async Task ConfirmMessageReceived(int? id)
        {
            Console.WriteLine("Message confirmation id = " + id);
            using (ChatContext context = new ChatContext())
            {
                var msg = context.Messages.FirstOrDefault(x => x.MessageId == id);
                if (msg != null)
                {
                    msg.IsSent = true;
                    await context.SaveChangesAsync();
                }
            }
        }
        async Task ProcessMessage(NetMessage message)
        {
            Console.WriteLine($"Получено сообщение от {message.NickNameFrom} для {message.NickNameTo} с командой {message.Command}: ");
            Console.WriteLine(message.Text);
            switch (message.Command)
            {
                case Command.Register: await Register(message); break;
                case Command.Message: await RelyMessage(message); break;
                case Command.Confirmation: await ConfirmMessageReceived(message.Id); break;
                default: break;
            }
        }
    }
}
