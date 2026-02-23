using System.Net;

namespace Server.Services.Abstracts
{
    internal interface ILoginAttemptTracker
    {
        void RecordFailedAttempt(IPAddress ip, string? login);
        bool IsBlocked(IPAddress ip, string? login);
        void ResetOnSuccess(IPAddress ip, string? login);
    }
}
