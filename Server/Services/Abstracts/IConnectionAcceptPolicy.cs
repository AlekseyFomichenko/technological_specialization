namespace Server.Services.Abstracts
{
    internal interface IConnectionAcceptPolicy
    {
        bool CanAccept(System.Net.IPAddress remoteAddress, int currentConnectionsFromIp, int totalConnections);
    }
}
