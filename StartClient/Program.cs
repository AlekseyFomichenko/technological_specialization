using ChatApp;
using System.Net;

namespace StartClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new Client<IPEndPoint>(new UdpMessageSourceClient(), "Vasya");
            client.Start();
        }
    }
}
