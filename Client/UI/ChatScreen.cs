using Client.Services;

namespace Client.UI
{
    internal static class ChatScreen
    {
        public static async Task RunAsync(AppSession session, CancellationToken cancellationToken)
        {
            session.ChatClient.TextMessageReceived += (_, payload) =>
                Console.WriteLine($"[From {payload.SenderId}] {payload.Content}");

            Console.WriteLine("Chat. Commands: /send <receiverId> <text>  /file <receiverId> <path>  /quit");
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = Console.ReadLine();
                if (line is null)
                    break;
                var parts = line.TrimStart().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;
                if (parts[0] == "/quit")
                    break;
                if (parts[0] == "/send" && parts.Length >= 3 && Guid.TryParse(parts[1], out var receiverId))
                {
                    var result = await session.ChatClient.SendMessageAsync(receiverId, parts[2], cancellationToken).ConfigureAwait(false);
                    Console.WriteLine(result.Success ? "Sent." : $"{result.ErrorCode}: {result.ErrorMessage}");
                    continue;
                }
                if (parts[0] == "/file" && parts.Length >= 3 && Guid.TryParse(parts[1], out var fileReceiverId))
                {
                    var path = parts[2];
                    var result = await session.FileClient.SendFileAsync(path, fileReceiverId, cancellationToken).ConfigureAwait(false);
                    Console.WriteLine(result.Success ? "File sent." : $"{result.ErrorCode}: {result.ErrorMessage}");
                    continue;
                }
                Console.WriteLine("Unknown command or invalid args.");
            }
        }
    }
}
