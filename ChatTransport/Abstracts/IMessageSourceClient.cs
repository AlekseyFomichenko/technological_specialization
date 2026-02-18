using System;
using System.Net;
using System.Threading.Tasks;
using ChatContracts;

namespace ChatTransport.Abstracts
{
    public interface IMessageSourceClient
    {
        Task SendAsync(NetMessage message, IPEndPoint? ep);
        NetMessage Receive(ref IPEndPoint ep);
        IPEndPoint CreateEndPoint();
        IPEndPoint GetServer();
    }
}
