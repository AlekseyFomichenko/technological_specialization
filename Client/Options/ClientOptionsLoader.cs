using Microsoft.Extensions.Configuration;

namespace Client.Options
{
    public static class ClientOptionsLoader
    {
        public static ClientOptions Load(string[]? args = null)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var options = new ClientOptions();
            config.GetSection("Client").Bind(options);
            ApplyArgs(options, args ?? Array.Empty<string>());
            return options;
        }

        private static void ApplyArgs(ClientOptions options, string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--host" && i + 1 < args.Length)
                {
                    options.ServerAddress = args[i + 1];
                    i++;
                }
                else if (args[i] == "--port" && i + 1 < args.Length && int.TryParse(args[i + 1], out int port))
                {
                    options.ServerPort = port;
                    i++;
                }
            }
        }
    }
}
