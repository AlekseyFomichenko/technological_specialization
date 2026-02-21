namespace Server.Services.Abstracts
{
    internal interface ILoginAttemptTracker
    {
        void RecordFailedAttempt(System.Net.IPAddress ip, string? login);
        bool IsBlocked(System.Net.IPAddress ip, string? login);
        void ResetOnSuccess(System.Net.IPAddress ip, string? login);
    }
}
