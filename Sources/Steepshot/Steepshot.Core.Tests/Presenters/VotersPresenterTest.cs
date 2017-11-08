using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Tests.Presenters
{
    [TestFixture]
    public class VotersPresenterTest : BaseTests
    {
        [Test, Sequential]
        public async Task GetPostVotersTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName, [Values("@steepshot/steepshot-some-stats-and-explanations", "@joseph.kalu/4k-photo-test-2017-10-10-07-15-42")] string url)
        {
            BasePresenter.SwitchChain(apiName);
            var presenter = new UserFriendPresenter();
            Assert.IsTrue(presenter.Count == 0);
            var errors = await presenter.TryLoadNextPostVoters(url);
            Assert.IsTrue(errors == null || errors.Count == 0);
            Assert.IsTrue(presenter.Count > 0);
        }
    }
}