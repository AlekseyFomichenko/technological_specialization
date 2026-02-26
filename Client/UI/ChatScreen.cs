using System.Collections.Concurrent;
using Client.Services;

namespace Client.UI
{
    public sealed class ChatScreen
    {
        private readonly AppSession _session;
        private readonly ConcurrentQueue<string> _incomingQueue = new();

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
                Guid? currentReceiverId = await PromptReceiverIdAsync(cancellationToken).ConfigureAwait(false);
                if (!currentReceiverId.HasValue)
                    return;

                PrintHelp();
                while (!cancellationToken.IsCancellationRequested)
                {
                    FlushIncoming();
                    var line = Console.ReadLine();
                    if (line is null)
                        break;
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0)
                        continue;
                    if (trimmed.StartsWith("/", StringComparison.Ordinal))
                    {
                        var (exit, newReceiverId) = await HandleCommandAsync(trimmed, currentReceiverId, cancellationToken).ConfigureAwait(false);
                        if (newReceiverId.HasValue)
                            currentReceiverId = newReceiverId;
                        if (exit)
                            break;
                    }
                    else
                    {
                        await SendMessageToCurrentAsync(currentReceiverId.Value, trimmed, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                _session.ChatClient.TextMessageReceived -= OnTextMessageReceived;
                _session.FileClient.FileReceiveStarted -= OnFileReceiveStarted;
                _session.FileClient.FileReceiveCompleted -= OnFileReceiveCompleted;
            }
        }

        private void OnTextMessageReceived(object? sender, Shared.DTO.IncomingTextPayload payload)
        {
            _incomingQueue.Enqueue($"[{payload.SenderId}]: {payload.Content}");
        }

        private void OnFileReceiveStarted(object? sender, (string FileName, Guid SenderId) e)
        {
            _incomingQueue.Enqueue($"[file] {e.FileName} (from {e.SenderId})");
        }

        private void OnFileReceiveCompleted(object? sender, (string FileName, string SavedPath) e)
        {
            _incomingQueue.Enqueue($"[file] {e.FileName} saved to {e.SavedPath}");
        }

        private void FlushIncoming()
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            while (_incomingQueue.TryDequeue(out var s))
                Console.WriteLine(s);
            Console.ForegroundColor = prev;
        }

        private static async Task<Guid?> PromptReceiverIdAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Enter receiver Guid: ");
                var line = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrEmpty(line))
                    continue;
                if (Guid.TryParse(line, out var id))
                    return id;
                WriteError("Invalid Guid format.");
            }
            return null;
        }

        private async Task<(bool ExitRequested, Guid? NewReceiverId)> HandleCommandAsync(string line, Guid? currentReceiverId, CancellationToken cancellationToken)
        {
            var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLowerInvariant();
            var arg = parts.Length > 1 ? parts[1].Trim() : "";

            if (cmd == "/exit")
                return (true, null);
            if (cmd == "/help")
            {
                PrintHelp();
                return (false, null);
            }
            if (cmd == "/to")
            {
                if (string.IsNullOrEmpty(arg) || !Guid.TryParse(arg, out var id))
                {
                    WriteError("Usage: /to <receiverId>");
                    return (false, null);
                }
                WriteSystem($"Receiver set to {id}.");
                return (false, id);
            }
            if (cmd == "/file")
            {
                if (!currentReceiverId.HasValue)
                {
                    WriteError("Set receiver first with /to <receiverId>.");
                    return (false, null);
                }
                var path = ParsePath(arg);
                if (string.IsNullOrEmpty(path))
                {
                    WriteError("Usage: /file <path> (path may be in quotes)");
                    return (false, null);
                }
                var result = await _session.FileClient.SendFileAsync(path, currentReceiverId.Value, cancellationToken).ConfigureAwait(false);
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

        private async Task SendMessageToCurrentAsync(Guid receiverId, string content, CancellationToken cancellationToken)
        {
            var result = await _session.ChatClient.SendMessageAsync(receiverId, content, cancellationToken).ConfigureAwait(false);
            if (result.Success)
                WriteSystem("Sent.");
            else
                WriteError($"{result.ErrorCode}: {result.ErrorMessage}");
        }

        private static void PrintHelp()
        {
            WriteSystem("Commands: /to <receiverId>  /file <path>  /exit  /help");
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
