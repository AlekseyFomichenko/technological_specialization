namespace Server.Services
{
    internal class ServerSessionOptions
    {
        public int Port { get; set; } = 5001;
        public int InactivityTimeoutMinutes { get; set; } = 5;
    }
}
