using ChatServer.Core;
using ChatServer.Database;
using ChatTransport.Tcp;
using ChatTransport.Udp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ServerHost
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

            var udpPort = int.TryParse(config["UdpPort"], out var p) ? p : 12345;
            var tcpFilePort = int.TryParse(config["TcpFilePort"], out var tp) ? tp : 12346;
            var connectionString = config["ConnectionStrings:Default"]
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("ConnectionStrings:Default or ConnectionStrings__Default is required.");
                return;
            }

            var optionsBuilder = new DbContextOptionsBuilder<ChatContext>();
            optionsBuilder.UseNpgsql(connectionString).UseLazyLoadingProxies();
            var contextFactory = new ChatContextFactory(optionsBuilder.Options);

            var messageSource = new UdpMessageSourceServer(udpPort);
            var server = new Server(messageSource, contextFactory);

            var fileReceiver = new TcpFileTransferReceiver();
            await fileReceiver.StartListenAsync(tcpFilePort);

            var udpTask = server.Run();
            var tcpTask = RunFileAcceptLoop(fileReceiver, server);

            Console.WriteLine($"Server starting UDP on port {udpPort}, TCP file port {tcpFilePort}");
            await Task.WhenAll(udpTask, tcpTask);
        }

        static async Task RunFileAcceptLoop(TcpFileTransferReceiver receiver, Server server)
        {
            while (true)
            {
                try
                {
                    var (stream, length) = await receiver.AcceptAsync();
                    try
                    {
                        await server.ProcessFileUploadAsync(stream, length);
                    }
                    finally
                    {
                        await stream.DisposeAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File accept error: " + ex.Message);
                }
            }
        }
    }
}
