using Server.Services.Abstracts;

namespace Server.Services
{
    internal class GuidTokenGenerator : ITokenGenerator
    {
        public string Generate()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
