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
    public class UdpMessageSourceClient : IMessageSourceClient<IPEndPoint>
    {

        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _udpEndPoint;
        public UdpMessageSourceClient(string IP = "127.0.0.1", int port = 12345)
        {
            _udpClient = new UdpClient();
            _udpEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        }

        public IPEndPoint CreateEndPoint() => new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
        public IPEndPoint GetServer() => _udpEndPoint;
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

        public Task SendAsync(IPEndPoint message, IPEndPoint ep)
        {
            throw new NotImplementedException();
        }
    }
}
