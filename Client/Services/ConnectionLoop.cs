using System.Text.Json;
using Client.Protocol;
using Microsoft.Extensions.Logging;
using Shared.DTO;
using Shared.Models;
using Shared.Protocol;

namespace Client.Services
{
    internal sealed class ConnectionLoop
    {
        private readonly PacketReader _reader;
        private readonly ChatClient _chatClient;
        private readonly FileClient _fileClient;
        private readonly PendingResponse _pending;
        private readonly Action _onDisconnected;
        private readonly ILogger _logger;

        public ConnectionLoop(
            PacketReader reader,
            ChatClient chatClient,
            FileClient fileClient,
            PendingResponse pending,
            Action onDisconnected,
            ILogger logger)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
            _fileClient = fileClient ?? throw new ArgumentNullException(nameof(fileClient));
            _pending = pending ?? throw new ArgumentNullException(nameof(pending));
            _onDisconnected = onDisconnected ?? throw new ArgumentNullException(nameof(onDisconnected));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var (type, payload) = await _reader.ReadPacketAsync(cancellationToken).ConfigureAwait(false);

                    switch (type)
                    {
                        case MessageType.TextMessage:
                            var textPayload = JsonSerializer.Deserialize<IncomingTextPayload>(payload, ClientProtocolConstants.JsonOptions);
                            if (textPayload is not null)
                                _chatClient.RaiseTextMessageReceived(textPayload);
                            break;
                        case MessageType.FileStart:
                            var fileAvailable = JsonSerializer.Deserialize<FileAvailablePayload>(payload, ClientProtocolConstants.JsonOptions);
                            if (fileAvailable is not null)
                                _fileClient.BeginReceive(fileAvailable);
                            break;
                        case MessageType.FileChunk:
                            _fileClient.ReceiveChunk(payload);
                            break;
                        case MessageType.FileEnd:
                            _fileClient.EndReceive();
                            break;
                        case MessageType.Ack:
                            _pending.CompleteSuccess();
                            break;
                        case MessageType.FileStartAck:
                            _pending.CompleteSuccess();
                            break;
                        case MessageType.Error:
                            var err = JsonSerializer.Deserialize<ErrorPayload>(payload, ClientProtocolConstants.JsonOptions);
                            _logger.LogWarning("Server returned error: Code={Code}, Message={Message}", err?.Code, err?.Message);
                            _pending.CompleteError(err?.Code ?? "Error", err?.Message ?? "Unknown error");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ProtocolException ex)
            {
                _logger.LogWarning(ex, "Connection lost: ProtocolException - {Message}", ex.Message);
                _onDisconnected();
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Connection lost: IOException - {Message}", ex.Message);
                _onDisconnected();
            }
        }
    }
}
