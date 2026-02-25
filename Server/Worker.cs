using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Server.Services;
using Server.Services.Abstracts;

namespace Server
{
    internal sealed class Worker : BackgroundService
    {
        private readonly IClientConnectionManager _manager;
        private readonly IOptions<ServerSessionOptions> _options;
        private readonly ILogger<Worker> _logger;

        public Worker(
            IClientConnectionManager manager,
            IOptions<ServerSessionOptions> options,
            ILogger<Worker> logger)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            int port = _options.Value.Port;
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(ct).ConfigureAwait(false);
                    await _manager.AcceptAsync(client, ct).ConfigureAwait(false);
                }
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
