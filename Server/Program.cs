using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Abstracts;
using Server.Services;
using Server.Services.Abstracts;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var config = builder.Configuration;
            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection") ?? string.Empty));

            builder.Services.Configure<FileStorageOptions>(config.GetSection("FileStorage"));
            builder.Services.Configure<ConnectionLimitOptions>(config.GetSection("Limits"));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<IFileMetadataRepository, FileMetadataRepository>();

            builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();
            builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
            builder.Services.AddSingleton<ITokenGenerator, GuidTokenGenerator>();
            builder.Services.AddScoped<ITokenValidator, SessionTokenValidator>();
            builder.Services.AddSingleton<ILoginAttemptTracker, LoginAttemptTracker>();
            builder.Services.AddSingleton<IConnectionAcceptPolicy, ConnectionAcceptPolicy>();
            builder.Services.AddSingleton<IMessageDelivery, MessageDeliveryStub>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<MessageService>();
            builder.Services.AddScoped<FileTransferService>();

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}