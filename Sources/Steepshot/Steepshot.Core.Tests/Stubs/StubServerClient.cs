using Steepshot.Core.Clients;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubServerClient : BaseServerClient
    {
        public StubServerClient(string url)
        {
            HttpClient = new StubExtendedHttpClient();
            BaseUrl = url;
            EnableRead = true;
        }
    }
}
