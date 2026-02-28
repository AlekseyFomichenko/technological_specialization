using System.Collections.Concurrent;
using Client.Services;
using Shared.DTO;

namespace Client.UI
{
    public sealed class ChatScreen
    {
        private const int MinReceiverLoginLength = 3;
        private const int MaxReceiverLoginLength = 100;

        private readonly AppSession _session;
        private readonly ConcurrentQueue<string> _incomingQueue = new();
        private readonly SemaphoreSlim _incomingSignal = new(0);

        public ChatScreen(AppSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _session.ChatClient.TextMessageReceived += OnTextMessageReceived;
            _session.FileClient.FileReceiveStarted += OnFileReceiveStarted;
            _session.FileClient.FileReceiveCompleted += OnFileReceiveCompleted;

            try
            {
                string? currentReceiverLogin = PromptReceiverLogin(cancellationToken);
                if (currentReceiverLogin is null)
                    return;

                PrintHelp();
                while (!cancellationToken.IsCancellationRequested)
                {
                    string? line;
                    try
                    {
                        line = await ReadLineWithIncomingAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    if (line is null)
                        break;
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0)
                        continue;
                    if (trimmed.StartsWith("/", StringComparison.Ordinal))
                    {
                        var (exit, newReceiverLogin) = await HandleCommandAsync(trimmed, currentReceiverLogin, cancellationToken).ConfigureAwait(false);
                        if (newReceiverLogin is not null)
                            currentReceiverLogin = newReceiverLogin;
                        if (exit)
                            break;
                    }
                    else
                    {
                        await SendMessageToCurrentAsync(currentReceiverLogin, trimmed, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _session.ChatClient.TextMessageReceived -= OnTextMessageReceived;
                _session.FileClient.FileReceiveStarted -= OnFileReceiveStarted;
                _session.FileClient.FileReceiveCompleted -= OnFileReceiveCompleted;
            }
        }

        private void OnTextMessageReceived(object? sender, IncomingTextPayload payload)
        {
            _incomingQueue.Enqueue($"[{payload.SenderLogin}]: {payload.Content}");
            _incomingSignal.Release();
        }

        private void OnFileReceiveStarted(object? sender, (string FileName, string SenderLogin) e)
        {
            _incomingQueue.Enqueue($"[file] {e.FileName} (from {e.SenderLogin})");
            _incomingSignal.Release();
        }

        private void OnFileReceiveCompleted(object? sender, (string FileName, string SavedPath) e)
        {
            _incomingQueue.Enqueue($"[file] {e.FileName} saved to {e.SavedPath}");
            _incomingSignal.Release();
        }

        private void FlushIncoming(string? currentLine = null)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            while (_incomingQueue.TryDequeue(out var s))
            {
                Console.WriteLine();
                Console.WriteLine(s);
                if (currentLine is not null)
                {
                    Console.Write("> ");
                    Console.Write(currentLine);
                }
            }
            Console.ForegroundColor = prev;
        }

        private async Task<string?> ReadLineWithIncomingAsync(CancellationToken cancellationToken)
        {
            var currentLine = new System.Text.StringBuilder();
            Console.Write("> ");
            while (!cancellationToken.IsCancellationRequested)
            {
                FlushIncoming(currentLine.Length > 0 ? currentLine.ToString() : "");
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return currentLine.ToString();
                    }
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (currentLine.Length > 0)
                        {
                            currentLine.Length--;
                            Console.Write("\b \b");
                        }
                        continue;
                    }
                    if (key.Key == ConsoleKey.C && (key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        Console.WriteLine();
                        return null;
                    }
                    if (!char.IsControl(key.KeyChar))
                    {
                        currentLine.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                    continue;
                }
                await _incomingSignal.WaitAsync(TimeSpan.FromMilliseconds(50), cancellationToken).ConfigureAwait(false);
            }
            return null;
        }

        private static string? PromptReceiverLogin(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Enter receiver login: ");
                var line = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrEmpty(line))
                    continue;
                if (!TryValidateReceiverLogin(line, out var error))
                {
                    WriteError(error ?? "Invalid receiver login.");
                    continue;
                }
                return line;
            }
            return null;
        }

        private static bool TryValidateReceiverLogin(string login, out string? error)
        {
            if (login.Length < MinReceiverLoginLength || login.Length > MaxReceiverLoginLength)
            {
                error = $"Receiver login must be between {MinReceiverLoginLength} and {MaxReceiverLoginLength} characters.";
                return false;
            }
            if (login.Contains(' '))
            {
                error = "Receiver login must not contain spaces.";
                return false;
            }
            error = null;
            return true;
        }

        private async Task<(bool ExitRequested, string? NewReceiverLogin)> HandleCommandAsync(string line, string? currentReceiverLogin, CancellationToken cancellationToken)
        {
            var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLowerInvariant();
            var arg = parts.Length > 1 ? parts[1].Trim() : "";

            if (cmd == "/exit")
                return (true, null);
            if (cmd == "/me")
            {
                var login = _session.SessionContext.Login;
                if (login is not null)
                    WriteSystem($"Your login: {login}");
                else
                    WriteError("Not logged in.");
                return (false, null);
            }
            if (cmd == "/help")
            {
                PrintHelp();
                return (false, null);
            }
            if (cmd == "/to")
            {
                if (string.IsNullOrEmpty(arg))
                {
                    WriteError("Usage: /to <receiverLogin>");
                    return (false, null);
                }
                if (!TryValidateReceiverLogin(arg, out var error))
                {
                    WriteError(error ?? "Invalid receiver login.");
                    return (false, null);
                }
                WriteSystem($"Receiver set to {arg}.");
                return (false, arg);
            }
            if (cmd == "/file")
            {
                if (currentReceiverLogin is null)
                {
                    WriteError("Set receiver first with /to <receiverLogin>.");
                    return (false, null);
                }
                var path = ParsePath(arg);
                if (string.IsNullOrEmpty(path))
                {
                    WriteError("Usage: /file <path> (path may be in quotes)");
                    return (false, null);
                }
                var result = await _session.FileClient.SendFileAsync(path, currentReceiverLogin, cancellationToken).ConfigureAwait(false);
                if (result.Success)
                    WriteSystem("File sent.");
                else
                    WriteError($"{result.ErrorCode}: {result.ErrorMessage}");
                return (false, null);
            }
            WriteError("Unknown command. Use /help.");
            return (false, null);
        }

        private static string? ParsePath(string arg)
        {
            var t = arg.Trim();
            if (t.Length == 0)
                return null;
            if (t.Length >= 2 && t[0] == '"' && t[^1] == '"')
                return t[1..^1].Trim();
            return t;
        }

        private async Task SendMessageToCurrentAsync(string receiverLogin, string content, CancellationToken cancellationToken)
        {
            var result = await _session.ChatClient.SendMessageAsync(receiverLogin, content, cancellationToken).ConfigureAwait(false);
            if (result.Success)
                WriteSystem("Sent.");
            else
                WriteError($"{result.ErrorCode}: {result.ErrorMessage}");
        }

        private static void PrintHelp()
        {
            WriteSystem("Commands: /to <receiverLogin>  /file <path>  /me  /exit  /help");
            WriteSystem("Example: /file \"C:\\My folder\\doc.pdf\"");
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
