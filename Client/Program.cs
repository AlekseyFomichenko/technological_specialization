using Client.Options;
using Client.Services;
using Client.UI;

namespace Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var options = ClientOptionsLoader.Load(args);
        using var session = new AppSession(options);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        session.OnDisconnected += (_, _) =>
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Disconnected from server.");
            Console.ForegroundColor = prev;
            cts.Cancel();
        };

        try
        {
            await session.ConnectAsync(cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connect failed: {ex.Message}");
            return;
        }

        var mainMenu = new MainMenu(session);
        var exit = await mainMenu.RunAsync(cts.Token).ConfigureAwait(false);
        if (exit)
            return;

        session.StartReadLoop(cts.Token);
        var chatScreen = new ChatScreen(session);
        await chatScreen.RunAsync(cts.Token).ConfigureAwait(false);
    }
}
