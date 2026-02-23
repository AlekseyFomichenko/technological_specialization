using System.Net;
using Microsoft.Extensions.Options;
using Server.Services.Abstracts;

namespace Server.Services
{
    internal class ConnectionAcceptPolicy : IConnectionAcceptPolicy
    {
        private readonly ConnectionLimitOptions _options;

        public ConnectionAcceptPolicy(IOptions<ConnectionLimitOptions> options)
        {
            _options = options.Value;
        }

        public bool CanAccept(IPAddress remoteAddress, int currentConnectionsFromIp, int totalConnections)
        {
            return currentConnectionsFromIp < _options.MaxConnectionsPerIp
                && totalConnections < _options.MaxConnections;
        }
    }
}
