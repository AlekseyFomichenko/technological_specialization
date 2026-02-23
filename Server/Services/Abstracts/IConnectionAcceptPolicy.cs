using System.Net;

namespace Server.Services.Abstracts
{
    internal interface IConnectionAcceptPolicy
    {
        bool CanAccept(IPAddress remoteAddress, int currentConnectionsFromIp, int totalConnections);
    }
}
