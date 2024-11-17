using Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                int temp = i;
                Task.Run(() => Client.TaskClient(temp));
            }
            Console.ReadKey();
        }
    }
}
