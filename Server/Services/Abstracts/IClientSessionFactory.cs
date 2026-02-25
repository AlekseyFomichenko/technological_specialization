using System.Net;
using Server.Protocol;

namespace Server.Services.Abstracts
{
    internal interface IClientSessionFactory
    {
        ClientSession Create(
            Stream stream,
            IPAddress? clientIp,
            Action<Guid> onTerminated,
            Action<Guid, Guid> onAuthenticated);
    }
}
