using Client.Services;

namespace Client.UI
{
    public sealed class MainMenu
    {
        private const int MinLoginLength = 3;
        private const int MaxLoginLength = 100;
        private const int MinPasswordLength = 6;
        private const int MaxPasswordLength = 128;

        private readonly AppSession _session;

        public MainMenu(AppSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// Returns true to exit app, false to proceed to chat.
        /// </summary>
        public async Task<bool> RunAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("1. Register  2. Login  3. Exit");
                var line = Console.ReadLine()?.Trim() ?? "";
                if (cancellationToken.IsCancellationRequested)
                    return true;
                if (line == "3")
                    return true;
                if (line == "1")
                {
                    await RunRegisterFlowAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }
                if (line == "2")
                {
                    var proceed = await RunLoginFlowAsync(cancellationToken).ConfigureAwait(false);
                    if (proceed)
                        return false;
                    continue;
                }
            }
            return true;
        }

        private async Task RunRegisterFlowAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Login: ");
                var login = Console.ReadLine()?.Trim() ?? "";
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (string.IsNullOrEmpty(login))
                {
                    WriteError("Login is required.");
                    continue;
                }
                if (login.Length < MinLoginLength || login.Length > MaxLoginLength)
                {
                    WriteError($"Login must be between {MinLoginLength} and {MaxLoginLength} characters.");
                    continue;
                }
                Console.Write("Password: ");
                var password = ReadPassword();
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (string.IsNullOrEmpty(password))
                {
                    WriteError("Password is required.");
                    continue;
                }
                if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
                {
                    WriteError($"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters.");
                    continue;
                }
                var result = await _session.AuthClient.RegisterAsync(login, password, cancellationToken).ConfigureAwait(false);
                if (result.Success)
                {
                    WriteSystem("Registered. Use Login to see your Id.");
                    return;
                }
                WriteError($"{result.ErrorCode}: {result.ErrorMessage}");
            }
        }

        private async Task<bool> RunLoginFlowAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Login: ");
                var login = Console.ReadLine()?.Trim() ?? "";
                if (cancellationToken.IsCancellationRequested)
                    return false;
                if (string.IsNullOrEmpty(login))
                {
                    WriteError("Login is required.");
                    continue;
                }
                if (login.Length < MinLoginLength || login.Length > MaxLoginLength)
                {
                    WriteError($"Login must be between {MinLoginLength} and {MaxLoginLength} characters.");
                    continue;
                }
                Console.Write("Password: ");
                var password = ReadPassword();
                if (cancellationToken.IsCancellationRequested)
                    return false;
                if (string.IsNullOrEmpty(password))
                {
                    WriteError("Password is required.");
                    continue;
                }
                if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
                {
                    WriteError($"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters.");
                    continue;
                }
                var result = await _session.AuthClient.LoginAsync(login, password, cancellationToken).ConfigureAwait(false);
                if (result.Success)
                {
                    WriteSystem("Logged in.");
                    var userId = _session.SessionContext.UserId;
                    if (userId.HasValue)
                        WriteSystem($"Your Id: {userId.Value}");
                    return true;
                }
                WriteError($"{result.ErrorCode}: {result.ErrorMessage}");
            }
            return false;
        }

        private static string ReadPassword()
        {
            var pass = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    pass = pass[..^1];
                else if (!char.IsControl(key.KeyChar))
                    pass += key.KeyChar;
            }
            Console.WriteLine();
            return pass;
        }

        private static void WriteError(string message)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = prev;
        }

        private static void WriteSystem(string message)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = prev;
        }
    }
}
