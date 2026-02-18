using System.Text;
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
                    var (stream, firstByte) = await receiver.AcceptConnectionAsync();
                    try
                    {
                        if (firstByte == TcpFileTransferReceiver.ModeUpload)
                        {
                            var lengthBytes = new byte[8];
                            lengthBytes[0] = firstByte;
                            await stream.ReadExactlyAsync(lengthBytes.AsMemory(1, 7));
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(lengthBytes);
                            var length = BitConverter.ToInt64(lengthBytes, 0);
                            await server.ProcessFileUploadAsync(stream, length);
                        }
                        else if (firstByte == TcpFileTransferReceiver.ModeDownload)
                        {
                            var fileIdBytes = new byte[4];
                            await stream.ReadExactlyAsync(fileIdBytes);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(fileIdBytes);
                            var fileId = BitConverter.ToInt32(fileIdBytes, 0);
                            var nickLenBytes = new byte[2];
                            await stream.ReadExactlyAsync(nickLenBytes);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(nickLenBytes);
                            var nickLen = BitConverter.ToInt16(nickLenBytes, 0);
                            if (nickLen < 0 || nickLen > 512)
                            {
                                continue;
                            }
                            var nickBytes = new byte[nickLen];
                            await stream.ReadExactlyAsync(nickBytes);
                            var requesterNick = Encoding.UTF8.GetString(nickBytes);
                            await server.ProcessFileDownloadRequestAsync(stream, fileId, requesterNick);
                        }
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
