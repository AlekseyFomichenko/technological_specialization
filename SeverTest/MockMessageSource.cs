using EFSeminar.Abstracts;
using EFSeminar.Models;
using EFSeminar.Services;
using System.Net;

namespace SeverTest
{
    internal class MockMessageSource : IMessageSource
    {
        private Server _server;
        private IPEndPoint _iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private Queue<NetMessage> _messages = new Queue<NetMessage> ();

        public MockMessageSource()
        {
            _server = new Server (this);
            _messages.Enqueue (new NetMessage() { Command = Command.Register, NickNameFrom = "Vasya"});
            _messages.Enqueue(new NetMessage() { Command = Command.Register, NickNameFrom = "Elena" });
            _messages.Enqueue(new NetMessage() { Command = Command.Message, NickNameFrom = "Elena", NickNameTo = "Vasya", Text = "From Elena" });
            _messages.Enqueue(new NetMessage() { Command = Command.Message, NickNameFrom = "Vasya", NickNameTo = "Elena", Text = "From Vasya" });
        }
        public void AddServer(Server server)
        {
            _server = server;
        }
        public NetMessage Receive(ref IPEndPoint ep)
        {
            ep = _iPEndPoint;
            if (_messages.Count == 0)
            {
                _server.Stop();
                return null;
            }
            return _messages.Dequeue();
        }

        public Task SendAsync(NetMessage message, IPEndPoint ep)
        {
            throw new NotImplementedException();
        }
    }
}
