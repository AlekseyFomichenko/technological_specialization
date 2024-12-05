using System.Net;
using System.Text.Json;

namespace ChatCommon.Models
{
    public enum Command
    {
        Register,
        Message,
        Confirmation
    }
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
