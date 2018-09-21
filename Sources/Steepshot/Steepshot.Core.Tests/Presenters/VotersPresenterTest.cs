using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Tests.Presenters
{
    [TestFixture]
    public class VotersPresenterTest : BaseTests
    {
        [Test]
        [TestCase(KnownChains.Steem, "@steepshot/steepshot-some-stats-and-explanations")]
        [TestCase(KnownChains.Golos, "@joseph.kalu/4k-photo-test-2017-10-10-07-15-42")]
        public async Task GetPostVotersTest(KnownChains apiName, string url)
        {
            var presenter = new UserFriendPresenter() { VotersType = VotersType.All };
            presenter.SetClient(Api[apiName]);
            Assert.IsTrue(presenter.Count == 0);
            var exception = await presenter.TryLoadNextPostVotersAsync(url);
            Assert.IsTrue(exception == null);
            Assert.IsTrue(presenter.Count > 0);
        }
    }
}