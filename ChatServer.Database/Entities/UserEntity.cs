using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Database.Entities
{
    public class UserEntity
    {
        public virtual List<MessageEntity> MessagesTo { get; set; } = new();
        public virtual List<MessageEntity> MessagesFrom { get; set; } = new();
        public int Id { get; set; }
        public string? FullName { get; set; }
    }
}
