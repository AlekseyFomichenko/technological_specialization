using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Server.Protocol;
using Server.Services.Abstracts;
using Shared.Models;

namespace Server.Services
{
    internal sealed class ClientConnectionManager : IClientConnectionManager, IMessageDelivery
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConnectionAcceptPolicy _policy;
        private readonly ILogger<ClientConnectionManager> _logger;
        private readonly ConcurrentDictionary<Guid, (TcpClient Client, ClientSession Session, IServiceScope Scope, IPAddress RemoteIp)> _byConnectionId = new();
        private readonly ConcurrentDictionary<Guid, ClientSession> _byUserId = new();
        private readonly ConcurrentDictionary<Guid, Guid> _userIdByConnectionId = new();

        public ClientConnectionManager(
            IServiceScopeFactory scopeFactory,
            IConnectionAcceptPolicy policy,
            ILogger<ClientConnectionManager> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AcceptAsync(TcpClient client, CancellationToken cancellationToken = default)
        {
            if (client?.Client?.RemoteEndPoint is not IPEndPoint remoteEndPoint)
            {
                client?.Dispose();
                await Task.CompletedTask.ConfigureAwait(false);
                return;
            }

            IPAddress ip = remoteEndPoint.Address;
            int currentFromIp = _byConnectionId.Values.Count(x => x.RemoteIp.Equals(ip));
            int total = _byConnectionId.Count;
            if (!_policy.CanAccept(ip, currentFromIp, total))
            {
                client.Dispose();
                await Task.CompletedTask.ConfigureAwait(false);
                return;
            }

            IServiceScope? scope = null;
            try
            {
                Stream stream = client.GetStream();
                scope = _scopeFactory.CreateScope();
                IClientSessionFactory factory = scope.ServiceProvider.GetRequiredService<IClientSessionFactory>();

                ClientSession session = factory.Create(
                    stream,
                    ip,
                    OnTerminated,
                    OnAuthenticated);

                Guid connectionId = session.ConnectionId;
                _byConnectionId[connectionId] = (client, session, scope, ip);
                _ = session.RunAsync(cancellationToken);
                await Task.CompletedTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to accept connection from {Address}", ip);
                scope?.Dispose();
                try { client.Dispose(); } catch { /* ignore */ }
                await Task.CompletedTask.ConfigureAwait(false);
            }
        }

        private void OnTerminated(Guid connectionId)
        {
            if (!_byConnectionId.TryRemove(connectionId, out var entry))
                return;
            if (_userIdByConnectionId.TryRemove(connectionId, out Guid userId))
                _byUserId.TryRemove(userId, out _);
            try { entry.Scope.Dispose(); } catch (Exception ex) { _logger.LogDebug(ex, "Scope dispose failed for {ConnectionId}", connectionId); }
            try { entry.Client.Dispose(); } catch (Exception ex) { _logger.LogDebug(ex, "Client dispose failed for {ConnectionId}", connectionId); }
        }

        private void OnAuthenticated(Guid connectionId, Guid userId)
        {
            if (_byConnectionId.TryGetValue(connectionId, out var entry))
            {
                _byUserId[userId] = entry.Session;
                _userIdByConnectionId[connectionId] = userId;
            }
        }

        public async Task<bool> SendToUserAsync(Guid userId, MessageType messageType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
        {
            if (_byUserId.TryGetValue(userId, out ClientSession? session))
                return await session.TrySendAsync(messageType, payload, cancellationToken).ConfigureAwait(false);
            return false;
        }
    }
}
