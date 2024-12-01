using EFSeminar.Models;
using System.Net;

namespace EFSeminar.Abstracts
{
    public interface IMessageSource
    {
        Task SendAsync(NetMessage message, IPEndPoint ep);
        NetMessage Receive(ref IPEndPoint ep);
    }
}
