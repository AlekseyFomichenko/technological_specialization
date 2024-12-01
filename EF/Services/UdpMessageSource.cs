using EFSeminar.Models;
using EFSeminar.Abstracts;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EFSeminar.Services
{
    public class UdpMessageSource : IMessageSource
    {
        private readonly UdpClient _udpClient;
        public UdpMessageSource()
        {
            _udpClient = new UdpClient(12345);
        }
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
