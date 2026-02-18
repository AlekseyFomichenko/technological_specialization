using System.Net;
using ChatContracts;
using ChatTransport.Abstracts;
using Microsoft.Extensions.Configuration;

namespace Client
{
    public class ClientApp
    {
        private readonly IMessageSourceClient _messageSource;
        private readonly string _serverHost;
        private readonly int _tcpFilePort;
        private string? _nick;
        private bool _running = true;

        public ClientApp(IMessageSourceClient messageSource, IConfiguration config)
        {
            _messageSource = messageSource;
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
                        case Command.Error:
                            Console.WriteLine($"[Error] {(msg.ErrorCode?.ToString() ?? "?")}: {msg.ErrorDescription ?? ""}");
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
                var colon = line.IndexOf(':');
                if (colon <= 0)
                {
                    Console.WriteLine("Use: recipient: message");
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

        private static void PrintMessage(NetMessage msg)
        {
            Console.WriteLine($"[{msg.Date:HH:mm}] {msg.NickNameFrom} -> {msg.NickNameTo}: {msg.Text ?? ""}");
        }
    }
}
