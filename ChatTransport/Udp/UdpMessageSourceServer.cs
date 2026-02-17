using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatContracts;
using ChatTransport.Abstracts;

namespace ChatTransport.Udp
{
    public class UdpMessageSourceServer : IMessageSourceServer<IPEndPoint>
    {
        private readonly UdpClient _udpClient;
        public UdpMessageSourceServer() => _udpClient = new UdpClient(12345);

        public IPEndPoint CopyEndPoint(IPEndPoint ep) => new IPEndPoint(ep.Address, ep.Port);

        public IPEndPoint CreateEndPoint() => new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);

        public NetMessage Receive(ref IPEndPoint ep)
        {
            byte[] data = _udpClient.Receive(ref ep);
            string str = Encoding.UTF8.GetString(data);
            return NetMessage.DeserializeMessageFromJson(str) ?? new NetMessage();
        }

        public async Task SendAsync(NetMessage message, IPEndPoint ep)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message.SerializeMessageToJson());
            await _udpClient.SendAsync(buffer, buffer.Length, ep);
        }
    }
}
