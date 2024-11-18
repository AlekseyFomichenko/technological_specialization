using Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.CompilerServices;

namespace Server
{
    internal class ServerCode
    {
        static private CancellationTokenSource cts = new CancellationTokenSource();
        static private CancellationToken ct = cts.Token;
        public static void Server()
        {
            using (UdpClient server = new UdpClient(12345))
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
                Console.Clear();
                Console.WriteLine("Waiting for connecting...");
                while (!ct.IsCancellationRequested)
                {
                    byte[] buffer = server.Receive(ref localEndPoint);
                    if (buffer == null) cts.Cancel();
                    string messageText = Encoding.UTF8.GetString(buffer);
                    Message? message = Message.DeserializeMessageFromJson(messageText);
                    if (!message.Text.Equals("Exit"))
                    {
                        message?.PrintMessageInfo();
                        server.Send(Encoding.UTF8.GetBytes("Сообщение доставлено!"), localEndPoint);
                    }
                    else cts.Cancel();
                }
            }
            ct.ThrowIfCancellationRequested();
        }
    }
}
