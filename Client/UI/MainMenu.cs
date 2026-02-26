using Client.Services;

namespace Client.UI
{
    internal static class MainMenu
    {
        /// <summary>
        /// Returns true to exit app, false to proceed to chat.
        /// </summary>
        public static async Task<bool> RunAsync(AppSession session)
        {
            while (true)
            {
                Console.WriteLine("1. Register  2. Login  3. Exit");
                var line = Console.ReadLine()?.Trim() ?? "";
                if (line == "3")
                    return true;
                if (line == "1")
                {
                    Console.Write("Login: ");
                    var login = Console.ReadLine() ?? "";
                    Console.Write("Password: ");
                    var password = ReadPassword();
                    var result = await session.AuthClient.RegisterAsync(login, password).ConfigureAwait(false);
                    Console.WriteLine(result.Success ? "Registered." : $"{result.ErrorCode}: {result.ErrorMessage}");
                    continue;
                }
                if (line == "2")
                {
                    Console.Write("Login: ");
                    var login = Console.ReadLine() ?? "";
                    Console.Write("Password: ");
                    var password = ReadPassword();
                    var result = await session.AuthClient.LoginAsync(login, password).ConfigureAwait(false);
                    if (result.Success)
                    {
                        Console.WriteLine("Logged in.");
                        return false;
                    }
                    Console.WriteLine($"{result.ErrorCode}: {result.ErrorMessage}");
                    continue;
                }
            }
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
    }
}
