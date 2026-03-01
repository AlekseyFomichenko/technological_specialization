namespace Server.Options
{
    internal class ServerSessionOptions
    {
        public int Port { get; set; } = 5001;
        public string IpAddress { get; set; } = "0.0.0.0";
        public int InactivityTimeoutMinutes { get; set; } = 5;
    }
}
