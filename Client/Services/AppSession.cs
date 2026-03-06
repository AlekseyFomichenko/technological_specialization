using System.Net.Sockets;
using System.Net;
using Client.Options;
using Microsoft.Extensions.Logging;
using Shared.Protocol;

namespace Client.Services
{
    public sealed class AppSession : IDisposable
    {
        private readonly ClientOptions _options;
        private readonly ILoggerFactory _loggerFactory;
        private TcpClient? _client;
        private Stream? _stream;
        private PacketReader? _reader;
        private PacketWriter? _writer;
        private SessionContext? _sessionContext;
        private PendingResponse? _pending;
        private AuthClient? _authClient;
        private ChatClient? _chatClient;
        private FileClient? _fileClient;
        private ConnectionLoop? _connectionLoop;
        private Task? _readLoopTask;
        private readonly object _disposeLock = new();
        private bool _disposed;

        public AppSession(ClientOptions options, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public event EventHandler? OnDisconnected;

        public SessionContext SessionContext => _sessionContext ?? throw new InvalidOperationException("Not connected. Call Connect first.");
        public AuthClient AuthClient => _authClient ?? throw new InvalidOperationException("Not connected. Call Connect first.");
        public ChatClient ChatClient => _chatClient ?? throw new InvalidOperationException("Not connected. Call Connect first.");
        public FileClient FileClient => _fileClient ?? throw new InvalidOperationException("Not connected. Call Connect first.");

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_options.ServerAddress, _options.ServerPort, cancellationToken).ConfigureAwait(false);
            _stream = _client.GetStream();
            _reader = new PacketReader(_stream);
            _writer = new PacketWriter(_stream);
            _sessionContext = new SessionContext();
            _pending = new PendingResponse();
            _authClient = new AuthClient(_reader, _writer, _sessionContext);
            _chatClient = new ChatClient(_writer, _sessionContext, _pending);
            var connectionLoopLogger = _loggerFactory.CreateLogger("Client.ConnectionLoop");
            var fileClientLogger = _loggerFactory.CreateLogger("Client.FileClient");
            _fileClient = new FileClient(_writer, _options, _pending, _sessionContext, fileClientLogger);
            _connectionLoop = new ConnectionLoop(_reader, _chatClient, _fileClient, _pending, HandleDisconnected, connectionLoopLogger);
        }

        public void StartReadLoop(CancellationToken cancellationToken = default)
        {
            if (_connectionLoop is null)
                throw new InvalidOperationException("Not connected.");
            _readLoopTask = Task.Run(() => _connectionLoop.RunAsync(cancellationToken), cancellationToken);
        }

        private void HandleDisconnected()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;
                try
                {
                    _stream?.Dispose();
                    _client?.Dispose();
                }
                catch { }
                _stream = null;
                _client = null;
                _disposed = true;
            }
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;
                try
                {
                    _stream?.Dispose();
                    _client?.Dispose();
                }
                catch { }
                _stream = null;
                _client = null;
                _disposed = true;
            }
        }
    }
}
