using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatContracts;

namespace ChatTransport.Abstracts
{
    public interface IMessageSourceClient<T>
    {
        Task SendAsync(NetMessage message, T? ep);
        NetMessage Receive(ref T ep);
        T CreateEndPoint();
        T GetServer();
    }
}
