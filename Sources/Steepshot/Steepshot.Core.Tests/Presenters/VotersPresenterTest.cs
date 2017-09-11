using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Tests.Presenters
{
    [TestFixture]
    public class VotersPresenterTest : BaseTests
    {
        [Test, Sequential]
        public async Task GetPostVotersTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName, [Values("@steepshot/steepshot-some-stats-and-explanations", "@steepshot/steepshot-nekotorye-statisticheskie-dannye-i-otvety-na-voprosy")] string url)
        {
            BasePresenter.SwitchChain(apiName);
            var presenter = new VotersPresenter();
            Assert.IsNotNull(presenter.Voters);
            Assert.IsTrue(presenter.Voters.Count == 0);
            var errors = await presenter.TryLoadNext(url);
            Assert.IsTrue(errors == null || errors.Count == 0);
            Assert.IsTrue(presenter.Voters.Count > 0);
        }
    }
}