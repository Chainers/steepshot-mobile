using Steepshot.Core.HttpClient;

namespace Steepshot.Core.Presenters
{
    public class TestPresenter : BasePresenter
    {
        public  SteepshotApiClient OpenApi => Api;
    }
}
