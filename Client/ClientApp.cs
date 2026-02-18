using System.Net;
using ChatContracts;
using ChatTransport.Abstracts;
using ChatTransport.Tcp;
using Microsoft.Extensions.Configuration;

namespace Client
{
    public class ClientApp
    {
        private const long MaxFileSizeBytes = 500L * 1024 * 1024;
        private const int FileOfferTimeoutMs = 30000;

        private readonly IMessageSourceClient _messageSource;
        private readonly IFileTransferSender _fileSender;
        private readonly string _serverHost;
        private readonly int _tcpFilePort;
        private string? _nick;
        private bool _running = true;
        private TaskCompletionSource<NetMessage?>? _pendingFileOfferTcs;
        private readonly object _pendingFileOfferLock = new();

        public ClientApp(IMessageSourceClient messageSource, IConfiguration config)
        {
            _messageSource = messageSource;
            _fileSender = new TcpFileTransferSender();
            _serverHost = config["ServerHost"] ?? "127.0.0.1";
            _tcpFilePort = int.TryParse(config["ServerTcpFilePort"], out var tp) ? tp : 12346;
        }

        public async Task RunAsync()
        {
            while (!await LoginOrRegisterAsync())
            {
            }

            var receiveTask = ReceiveLoopAsync();
            var sendTask = SendLoopAsync();
            await Task.WhenAll(receiveTask, sendTask);
        }

        private static bool TryValidateNick(string? nick, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(nick))
            {
                error = "Nick required.";
                return false;
            }
            var trimmed = nick.Trim();
            if (trimmed.Contains(' '))
            {
                error = "Nick must not contain spaces.";
                return false;
            }
            if (trimmed.Length < NickValidation.MinLength || trimmed.Length > NickValidation.MaxLength)
            {
                error = $"Nick length must be {NickValidation.MinLength}-{NickValidation.MaxLength}.";
                return false;
            }
            return true;
        }

        private async Task<bool> LoginOrRegisterAsync()
        {
            Console.WriteLine("1 = Register, 2 = Login");
            var choice = Console.ReadLine()?.Trim();
            Console.Write("Nick: ");
            var nick = Console.ReadLine()?.Trim();
            Console.Write("Password: ");
            var password = ReadPassword();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Password required.");
                return false;
            }
            if (!TryValidateNick(nick, out var nickError))
            {
                Console.WriteLine(nickError);
                return false;
            }

            var cmd = choice == "1" ? Command.Register : Command.Login;
            var msg = new NetMessage
            {
                Command = cmd,
                NickNameFrom = nick,
                Password = password,
                Date = DateTime.UtcNow,
                EndPoint = _messageSource.GetServer()
            };
            await _messageSource.SendAsync(msg, _messageSource.GetServer());

            IPEndPoint from = _messageSource.CreateEndPoint();
            var response = _messageSource.Receive(ref from);
            if (response.Command == Command.Error)
            {
                Console.WriteLine($"Error ({(response.ErrorCode?.ToString() ?? "?")}): {response.ErrorDescription ?? ""}");
                return false;
            }

            _nick = nick;
            Console.WriteLine($"Logged in as {_nick}. History and new messages below.");
            if (response.Command == Command.Message)
                PrintMessage(response);
            return true;
        }

        private static string ReadPassword()
        {
            var pass = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    pass = pass[..^1];
                else if (!char.IsControl(key.KeyChar))
                    pass += key.KeyChar;
            }
            Console.WriteLine();
            return pass;
        }

        private async Task ReceiveLoopAsync()
        {
            var from = _messageSource.CreateEndPoint();
            while (_running)
            {
                try
                {
                    var msg = _messageSource.Receive(ref from);
                    switch (msg.Command)
                    {
                        case Command.Message:
                            PrintMessage(msg);
                            if (msg.Id != null)
                            {
                                var confirmation = new NetMessage
                                {
                                    Command = Command.Confirmation,
                                    Id = msg.Id,
                                    Date = DateTime.UtcNow
                                };
                                await _messageSource.SendAsync(confirmation, _messageSource.GetServer());
                            }
                            break;
                        case Command.Ack:
                            lock (_pendingFileOfferLock)
                            {
                                if (_pendingFileOfferTcs != null)
                                {
                                    _pendingFileOfferTcs.TrySetResult(null);
                                    _pendingFileOfferTcs = null;
                                }
                            }
                            break;
                        case Command.Error:
                            lock (_pendingFileOfferLock)
                            {
                                if (_pendingFileOfferTcs != null)
                                {
                                    _pendingFileOfferTcs.TrySetResult(msg);
                                    _pendingFileOfferTcs = null;
                                }
                                else
                                    Console.WriteLine($"[Error] {(msg.ErrorCode?.ToString() ?? "?")}: {msg.ErrorDescription ?? ""}");
                            }
                            break;
                        case Command.FileAvailable:
                            Console.WriteLine($"[File] From {msg.NickNameFrom}: {msg.FileName} (id={msg.FileId})");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Receive error: " + ex.Message);
                }
            }
        }

        private async Task SendLoopAsync()
        {
            while (_running)
            {
                var line = await Task.Run(() => Console.ReadLine());
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    _running = false;
                    break;
                }
                if (line.StartsWith("file ", StringComparison.OrdinalIgnoreCase))
                {
                    await SendFileAsync(line.Substring(5).Trim());
                    continue;
                }
                var colon = line.IndexOf(':');
                if (colon <= 0)
                {
                    Console.WriteLine("Use: recipient: message  or  file recipient: path");
                    continue;
                }
                var to = line[..colon].Trim();
                var text = line[(colon + 1)..].Trim();
                var msg = new NetMessage
                {
                    Command = Command.Message,
                    NickNameFrom = _nick,
                    NickNameTo = to,
                    Text = text,
                    Date = DateTime.UtcNow
                };
                await _messageSource.SendAsync(msg, _messageSource.GetServer());
            }
        }

        private async Task SendFileAsync(string recipientAndPath)
        {
            var colon = recipientAndPath.IndexOf(':');
            if (colon <= 0)
            {
                Console.WriteLine("Use: file recipient: path");
                return;
            }
            var to = recipientAndPath[..colon].Trim();
            var path = recipientAndPath[(colon + 1)..].Trim();
            if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Use: file recipient: path");
                return;
            }
            if (!File.Exists(path))
            {
                Console.WriteLine("File not found: " + path);
                return;
            }
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length == 0 || fileInfo.Length > MaxFileSizeBytes)
            {
                Console.WriteLine("File size must be 1..500 MB.");
                return;
            }
            var fileName = fileInfo.Name;
            var fileOffer = new NetMessage
            {
                Command = Command.FileOffer,
                NickNameFrom = _nick,
                NickNameTo = to,
                FileName = fileName,
                FileSize = fileInfo.Length,
                MimeType = null,
                Date = DateTime.UtcNow,
                EndPoint = _messageSource.GetServer()
            };
            await _messageSource.SendAsync(fileOffer, _messageSource.GetServer());

            TaskCompletionSource<NetMessage?> tcs;
            lock (_pendingFileOfferLock)
            {
                _pendingFileOfferTcs = new TaskCompletionSource<NetMessage?>();
                tcs = _pendingFileOfferTcs;
            }
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(FileOfferTimeoutMs));
            NetMessage? errorMsg = null;
            lock (_pendingFileOfferLock)
            {
                if (completed == tcs.Task && tcs.Task.IsCompletedSuccessfully)
                    errorMsg = tcs.Task.Result;
                else if (completed != tcs.Task)
                {
                    tcs.TrySetCanceled();
                    _pendingFileOfferTcs = null;
                    Console.WriteLine("File send timed out.");
                    return;
                }
                _pendingFileOfferTcs = null;
            }
            if (errorMsg != null)
            {
                Console.WriteLine("File send failed: " + (errorMsg.ErrorDescription ?? errorMsg.ErrorCode?.ToString() ?? "Error"));
                return;
            }
            try
            {
                await using (var stream = File.OpenRead(path))
                    await _fileSender.SendAsync(_serverHost, _tcpFilePort, stream, fileInfo.Length);
                Console.WriteLine("File sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("File send error: " + ex.Message);
            }
        }

        private static void PrintMessage(NetMessage msg)
        {
            Console.WriteLine($"[{msg.Date:HH:mm}] {msg.NickNameFrom} -> {msg.NickNameTo}: {msg.Text ?? ""}");
        }
    }
}
