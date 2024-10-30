using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Homeworks.Network_application_development
{
    internal class Program
    {
        private static IPEndPoint _ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);

        static void UpdReceiver()
        {
            using (UdpClient server = new UdpClient(12345))
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6644);
                int count = 0;
                while (count < 9000)
                {
                    byte[] data = server.Receive(ref localEndPoint);
                    for (int i = 0; i < data.Length; i++)
                    {
                        Console.Write(data[i] + " ");
                    }
                    count += data.Length;
                }
            }
        }
        static void UdpSender(byte b)
        {
            using (UdpClient client = new UdpClient())
            {
                client.Connect(_ep);
                for (int i = 0; i < 3000; i++)
                {
                    client.Send([b]);
                }
            }
        }
        static void Main(string[] args)
        {
            new Thread(UpdReceiver).Start();
            for (int i = 0; i < 3; i++)
            {
                byte b = (byte)i;
                new Thread(() => UdpSender(b)).Start();
            }
            Console.ReadLine();
        }
    }
}
