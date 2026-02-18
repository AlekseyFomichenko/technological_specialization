using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatContracts.Json;

namespace ChatContracts
{
    public class NetMessage
    {
        public int? Id { get; set; }
        public string? Text { get; set; }
        public DateTime Date { get; set; }
        public string? NickNameTo { get; set; }
        public string? NickNameFrom { get; set; }
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint? EndPoint { get; set; }
        public Command Command { get; set; }
        public string? Password { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public string? MimeType { get; set; }
        public int? FileId { get; set; }
        public ErrorCode? ErrorCode { get; set; }
        public string? ErrorDescription { get; set; }
        public string SerializeMessageToJson() => JsonSerializer.Serialize(this);
        public static NetMessage? DeserializeMessageFromJson(string message) => JsonSerializer.Deserialize<NetMessage>(message);
    }
}
