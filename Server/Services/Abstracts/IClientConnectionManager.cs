using System.Net.Sockets;

namespace Server.Services.Abstracts
{
    internal interface IClientConnectionManager
    {
        Task AcceptAsync(TcpClient client, CancellationToken cancellationToken = default);
    }
}
