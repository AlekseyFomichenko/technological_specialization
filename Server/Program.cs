using Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            var t = Task.Run(Server.TaskServer);
            Task.WaitAll(t);
        }
    }
}
