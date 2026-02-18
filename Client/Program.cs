using Microsoft.Extensions.Configuration;

namespace Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var host = config["ServerHost"] ?? "127.0.0.1";
            var udpPort = int.TryParse(config["ServerUdpPort"], out var p) ? p : 12345;

            var messageSource = new ChatTransport.Udp.UdpMessageSourceClient(host, udpPort);
            var app = new ClientApp(messageSource, config);
            await app.RunAsync();
        }
    }
}
