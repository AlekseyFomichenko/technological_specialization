using System.Collections.Concurrent;
using Server.Services.Abstracts;
using System.Net;
using System.Text;

namespace Server.Services
{
    internal class LoginAttemptTracker : ILoginAttemptTracker
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

        private readonly ConcurrentDictionary<string, List<DateTime>> _attempts = new();
        private static readonly object ListLock = new();

        public void RecordFailedAttempt(IPAddress? ip, string? login)
        {
            var now = DateTime.UtcNow;
            if (ip is not null)
                AddAttempt(KeyIp(ip), now);
            if (!string.IsNullOrEmpty(login))
                AddAttempt(KeyLogin(login), now);
        }

        public bool IsBlocked(IPAddress? ip, string? login)
        {
            if (ip is not null && CountInWindow(KeyIp(ip)) >= MaxAttempts)
                return true;
            if (!string.IsNullOrEmpty(login) && CountInWindow(KeyLogin(login)) >= MaxAttempts)
                return true;
            return false;
        }

        public void ResetOnSuccess(IPAddress? ip, string? login)
        {
            if (ip is not null)
                _attempts.TryRemove(KeyIp(ip), out _);
            if (!string.IsNullOrEmpty(login))
                _attempts.TryRemove(KeyLogin(login), out _);
        }

        private static string KeyIp(IPAddress ip) => "ip:" + (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString());
        private static string KeyLogin(string login) => "login:" + login.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormC);

        private void AddAttempt(string key, DateTime at)
        {
            var list = _attempts.GetOrAdd(key, _ => new List<DateTime>());
            lock (list)
            {
                list.Add(at);
                Prune(list, at - Window);
            }
        }

        private int CountInWindow(string key)
        {
            if (!_attempts.TryGetValue(key, out var list))
                return 0;
            lock (list)
            {
                Prune(list, DateTime.UtcNow - Window);
                return list.Count;
            }
        }

        private static void Prune(List<DateTime> list, DateTime before)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] < before)
                    list.RemoveAt(i);
            }
        }
    }
}
