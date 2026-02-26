using System.Text.Json;

namespace Client.Protocol
{
    public static class ClientProtocolConstants
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public const long MaxFileSizeBytes = 50L * 1024 * 1024;
    }
}
