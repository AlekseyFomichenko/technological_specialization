using ChatApp;
using System.Net;

namespace StartServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var s = new Server<IPEndPoint>(new UdpMessageSourceServer());
            s.Work();
        }
    }
}
