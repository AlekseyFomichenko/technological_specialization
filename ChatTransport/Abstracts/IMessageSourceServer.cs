using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChatContracts;

namespace ChatTransport.Abstracts
{
    public interface IMessageSourceServer<T>
    {
        Task SendAsync(NetMessage message, T ep);
        NetMessage Receive(ref T ep);
        T CreateEndPoint();
        T CopyEndPoint(IPEndPoint ep);
    }
}
