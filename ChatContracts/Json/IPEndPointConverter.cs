using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatContracts.Json
{
    public class IPEndPointConverter : JsonConverter<IPEndPoint?>
    {
        public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return null;
            var parts = s.Split(':');
            if (parts.Length != 2) return null;
            if (!System.Net.IPAddress.TryParse(parts[0], out var address)) return null;
            if (!int.TryParse(parts[1], out var port)) return null;
            return new IPEndPoint(address, port);
        }

        public override void Write(Utf8JsonWriter writer, IPEndPoint? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue($"{value.Address}:{value.Port}");
        }
    }
}
