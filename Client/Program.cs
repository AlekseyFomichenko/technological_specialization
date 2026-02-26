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

        session.OnDisconnected += (_, _) => Console.WriteLine("Disconnected from server.");

        try
        {
            await session.ConnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connect failed: {ex.Message}");
            return;
        }

        var exit = await MainMenu.RunAsync(session).ConfigureAwait(false);
        if (exit)
            return;

        using var cts = new CancellationTokenSource();
        session.StartReadLoop(cts.Token);
        await ChatScreen.RunAsync(session, cts.Token).ConfigureAwait(false);
    }
}
