using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddDbContext<ChatDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty));
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}