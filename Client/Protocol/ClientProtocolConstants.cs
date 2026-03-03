using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Protocol
{
    public static class ClientProtocolConstants
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
