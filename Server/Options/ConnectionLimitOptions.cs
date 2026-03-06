namespace Server.Options
{
    internal class ConnectionLimitOptions
    {
        public int MaxConnections { get; set; } = 100;
        public int MaxConnectionsPerIp { get; set; } = 3;
    }
}
