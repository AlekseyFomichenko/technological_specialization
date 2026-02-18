using System.Net.Sockets;
using System.Net;
using ChatTransport.Abstracts;

namespace ChatTransport.Tcp
{
    public class TcpFileTransferSender : IFileTransferSender
    {
        public async Task SendAsync(string host, int port, Stream content, long length, CancellationToken cancellationToken = default)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse(host), port, cancellationToken);
            await using var stream = client.GetStream();
            stream.WriteByte(TcpFileTransferReceiver.ModeUpload);
            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            await stream.WriteAsync(lengthBytes, cancellationToken);
            await content.CopyToAsync(stream, cancellationToken);
        }
    }
}
