namespace Shared.Protocol
{
    /// <summary>
    /// Формат пакета: [1 байт — тип сообщения][4 байта — длина payload, little-endian][N байт — payload].
    /// Длина payload не более <see cref="MaxPayloadSize"/>.
    /// </summary>
    public static class PacketFormat
    {
        public const int HeaderSize = 5;

        public const int MaxPayloadSize = 50 * 1024 * 1024;
    }
}
