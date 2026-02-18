using System.Net;
using System.Net.Sockets;
using ChatTransport.Abstracts;

namespace ChatTransport.Tcp
{
    public class TcpFileTransferReceiver : IFileTransferReceiver
    {
        private TcpListener? _listener;

        public async Task StartListenAsync(int port, CancellationToken cancellationToken = default)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            await Task.CompletedTask;
        }

        public const byte ModeUpload = 0;
        public const byte ModeDownload = 1;

        public async Task<(Stream Stream, long Length)> AcceptAsync(CancellationToken cancellationToken = default)
        {
            if (_listener == null)
                throw new InvalidOperationException("Call StartListenAsync first.");
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            var netStream = client.GetStream();
            var lengthBytes = new byte[8];
            await netStream.ReadExactlyAsync(lengthBytes, cancellationToken);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            var length = BitConverter.ToInt64(lengthBytes, 0);
            var stream = new TcpClientOwnedStream(netStream, client);
            return (stream, length);
        }

        public async Task<(Stream Stream, byte FirstByte)> AcceptConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_listener == null)
                throw new InvalidOperationException("Call StartListenAsync first.");
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            var netStream = client.GetStream();
            var firstByteBytes = new byte[1];
            await netStream.ReadExactlyAsync(firstByteBytes, cancellationToken);
            var stream = new TcpClientOwnedStream(netStream, client);
            return (stream, firstByteBytes[0]);
        }
    }

    internal sealed class TcpClientOwnedStream : Stream
    {
        private readonly Stream _inner;
        private readonly TcpClient _client;

        public TcpClientOwnedStream(Stream inner, TcpClient client)
        {
            _inner = inner;
            _client = client;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }

        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct) => await _inner.ReadAsync(buffer, ct);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _client.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
