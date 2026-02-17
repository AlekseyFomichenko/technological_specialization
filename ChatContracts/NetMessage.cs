using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatContracts
{
    public class NetMessage
    {
        public int? Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public string? NickNameTo { get; set; }
        public string? NickNameFrom { get; set; }
        public IPEndPoint? EndPoint { get; set; }
        public Command Command { get; set; }
        public string SerializeMessageToJson() => JsonSerializer.Serialize(this);
        public static NetMessage? DeserializeMessageFromJson(string message) => JsonSerializer.Deserialize<NetMessage>(message);
    }
}
