using Steepshot.Core.Clients;

namespace Steepshot.Core.Presenters
{
    public sealed class TestPresenter : BasePresenter
    {
        public SteepshotApiClient OpenApi => Api;
    }
}
