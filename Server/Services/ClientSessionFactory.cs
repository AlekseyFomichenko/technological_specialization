using System.Net;
using Microsoft.Extensions.Options;
using Server.Protocol;
using Server.Services.Abstracts;

namespace Server.Services
{
    internal sealed class ClientSessionFactory : IClientSessionFactory
    {
        private readonly IAuthService _authService;
        private readonly IMessageService _messageService;
        private readonly IFileTransferService _fileTransferService;
        private readonly IOptions<ServerSessionOptions> _sessionOptions;
        private readonly ILogger<ClientSession> _logger;

        public ClientSessionFactory(
            IAuthService authService,
            IMessageService messageService,
            IFileTransferService fileTransferService,
            IOptions<ServerSessionOptions> sessionOptions,
            ILogger<ClientSession> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _fileTransferService = fileTransferService ?? throw new ArgumentNullException(nameof(fileTransferService));
            _sessionOptions = sessionOptions ?? throw new ArgumentNullException(nameof(sessionOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ClientSession Create(
            Stream stream,
            IPAddress? clientIp,
            Action<Guid> onTerminated,
            Action<Guid, Guid> onAuthenticated)
        {
            return new ClientSession(
                stream,
                clientIp,
                onTerminated ?? throw new ArgumentNullException(nameof(onTerminated)),
                onAuthenticated ?? throw new ArgumentNullException(nameof(onAuthenticated)),
                _authService,
                _messageService,
                _fileTransferService,
                _sessionOptions,
                _logger);
        }
    }
}
