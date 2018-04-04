using Steepshot.Core.HttpClient;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubServerClient : BaseServerClient
    {
        public StubServerClient(JsonNetConverter converter, string url)
        {
            Gateway = new StubApiGateway();
            JsonConverter = converter;
            Gateway.BaseUrl = url;
            EnableRead = true;
        }
    }
}
