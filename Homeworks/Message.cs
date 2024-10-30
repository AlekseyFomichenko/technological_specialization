using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Network
{
    public class Message
    {
        public string? Text { get; set; }
        public DateTime DateTime { get; set; }
        public string? NicknameFrom { get; set; }
        public string? NicknameTo { get; set; }
        public string SerializeMessageToJson() => JsonSerializer.Serialize(this);
        public static void Deserialize(string message) => JsonSerializer.Deserialize<Message>(message);
    }
}
