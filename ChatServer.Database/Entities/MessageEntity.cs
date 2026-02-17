using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Database.Entities
{
    public class MessageEntity
    {
        public int? MessageId { get; set; }
        public string? Text { get; set; }
        public DateTime DateSend { get; set; }
        public bool IsSent { get; set; }
        public int? UserToId { get; set; }
        public int? UserFromId { get; set; }
        public virtual UserEntity? UserTo { get; set; }
        public virtual UserEntity? UserFrom { get; set; }
    }
}
