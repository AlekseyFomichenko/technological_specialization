using System.Buffers.Binary;
using System.IO;
using Shared.Models;
using Shared.Protocol;

namespace Server.Protocol
{
    internal sealed class PacketReader
    {
        private readonly Stream _stream;

        public PacketReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Читает один пакет: 1 байт тип, 4 байта длина (little-endian), N байт payload.
        /// При неизвестном типе или неверной длине — <see cref="ProtocolException"/>; при обрыве — <see cref="IOException"/>.
        /// </summary>
        public async Task<(MessageType Type, byte[] Payload)> ReadPacketAsync(CancellationToken cancellationToken = default)
        {
            byte[] headerBuf = new byte[PacketFormat.HeaderSize];
            await _stream.ReadExactlyAsync(headerBuf, cancellationToken).ConfigureAwait(false);

            MessageType type = (MessageType)headerBuf[0];
            if (!Enum.IsDefined(typeof(MessageType), type))
                throw new ProtocolException("Unknown message type.");

            int length = BinaryPrimitives.ReadInt32LittleEndian(headerBuf.AsSpan(1, 4));
            if (length < 0 || length > PacketFormat.MaxPayloadSize)
                throw new ProtocolException($"Payload length {length} is out of range [0, {PacketFormat.MaxPayloadSize}].");

            byte[] payload = new byte[length];
            if (length > 0)
                await _stream.ReadExactlyAsync(payload, cancellationToken).ConfigureAwait(false);

            return (type, payload);
        }
    }
}
