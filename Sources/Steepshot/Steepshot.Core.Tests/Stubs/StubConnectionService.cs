using Steepshot.Core.Services;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubConnectionService : IConnectionService
    {
        public bool IsConnectionAvailable()
        {
            return true;
        }
    }
}
