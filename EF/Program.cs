using EFSeminar.Models;
using Microsoft.EntityFrameworkCore;

namespace EFSeminar
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatContext>()
                .UseSqlServer("Server=.;Database=master;Trusted_Connection=True")
                .UseLazyLoadingProxies();
        }
    }
}
