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
            try
            {
                ServerCode.Server();
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Сервер завершил работу.");
            }
        }
    }
}
