using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Patterns
{
    public class Manager
    {
        private Server _server;
        public Manager(Server server) => _server = server;
        public TypeSend Execute(Message message, IPEndPoint iPEndPoint)
        {
            switch (message.Command)
            {
                case Commands.Delete: Delete(message.NicknameFrom); break;
                case Commands.Register: Register(message.NicknameTo, iPEndPoint); break;
                default: Send(message); break;
            }
            return TypeSend.Default;
        }
        public void Register(string userName, IPEndPoint ip)
        {
            if (_server.Users == null) _server.Users = [];
            _server.Users.Add(userName, ip);
            Console.WriteLine($"Пользователь {userName} зареган");
        }
        public void Delete(string userName) 
        {
            _server.Users.Remove(userName);
            Console.WriteLine($"Пользователь {userName} удалён");
        }
        public TypeSend Send(Message msg) 
        {
            if(string.IsNullOrEmpty(msg.NicknameTo)) return TypeSend.ToAll;
            else return TypeSend.ToOne;
        }
    }
}
