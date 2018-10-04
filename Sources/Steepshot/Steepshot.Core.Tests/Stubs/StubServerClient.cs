using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubServerClient : BaseServerClient
    {
        public StubServerClient(ExtendedHttpClient httpClient, ILogService logger, string baseUrl)
            : base(httpClient, logger, baseUrl)
        {
        }
    }
}
