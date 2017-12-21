using System;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Models.Requests;
using System.Threading.Tasks;

namespace Steepshot.Core.Tests.HttpClient
{
    [TestFixture]
    public class BaseClientTest : BaseTests
    {

        [Test]
        [TestCase(KnownChains.Steem, "@steepshot/steepshot-some-stats-and-explanations")]
        [TestCase(KnownChains.Golos, "@anatolich/utro-dobroe-gospoda-i-damy-khochu-chtoby-opyatx-bylo-leto-plyazh-i-solncze--2017-11-08-02-10-33")]
        public async Task GetPostVotersTest(KnownChains apiName, string url)
        {
            var count = 40;
            var request = new VotersRequest(url, VotersType.All)
            {
                Limit = count,
                Offset = string.Empty,

            };
            var responce = await Api[apiName].GetPostVoters(request, CancellationToken.None);
            Assert.IsTrue(responce.Success);
            Assert.IsTrue(responce.Result.Count == count);
        }

        [Test]
        [TestCase(KnownChains.Steem, "@steepshot/steepshot-some-stats-and-explanations")]
        [TestCase(KnownChains.Golos, "@steepshot/steepshot-nekotorye-statisticheskie-dannye-i-otvety-na-voprosy")]
        public async Task GetPostVotersCancelTestTest(KnownChains apiName, string url)
        {
            try
            {
                var count = 40;
                var request = new VotersRequest(url, VotersType.All)
                {
                    Limit = count,
                    Offset = string.Empty
                };

                var token = new CancellationTokenSource(50);
                var responce = await Api[apiName].GetPostVoters(request, token.Token);
                Assert.IsTrue(responce.Success);
                Assert.IsTrue(responce.Result.Count == count);
            }
            catch (OperationCanceledException)
            {
                Assert.Pass();
                // go up
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
