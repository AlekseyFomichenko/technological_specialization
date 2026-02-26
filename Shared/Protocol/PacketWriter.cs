using System.Buffers.Binary;
using Shared.Models;

namespace Shared.Protocol
{
    /// <summary>
    /// Записывает пакет: 1 байт тип, 4 байта длина payload (little-endian), N байт payload; затем flush после каждой записи.
    /// Размер payload не более <see cref="PacketFormat.MaxPayloadSize"/>.
    /// </summary>
    public sealed class PacketWriter
    {
        private readonly Stream _stream;

        public PacketWriter(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Записывает пакет и выполняет flush.
        /// </summary>
        public async Task WritePacketAsync(MessageType type, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
        {
            if (payload.Length > PacketFormat.MaxPayloadSize)
                throw new ArgumentOutOfRangeException(nameof(payload), $"Payload size {payload.Length} exceeds {PacketFormat.MaxPayloadSize}.");

            byte[] headerBuf = new byte[PacketFormat.HeaderSize];
            headerBuf[0] = (byte)type;
            BinaryPrimitives.WriteInt32LittleEndian(headerBuf.AsSpan(1, 4), payload.Length);

            await _stream.WriteAsync(headerBuf, cancellationToken).ConfigureAwait(false);
            if (payload.Length > 0)
                await _stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
