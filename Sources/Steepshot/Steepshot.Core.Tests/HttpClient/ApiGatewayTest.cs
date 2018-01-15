using System;
using System.IO;
using System.Threading;
using Steepshot.Core.HttpClient;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Steepshot.Core.Tests.HttpClient
{
    [TestFixture]
    public class ApiGatewayTest : BaseTests
    {
        readonly ApiGateway _api = new ApiGateway();

        [Test]
        public async Task NsfwCheckTest()
        {
            var stream = File.OpenRead(GetTestImagePath());
            var response = await _api.NsfwCheck(stream, CancellationToken.None);
            AssertResult(response);
            Assert.IsTrue(response.IsSuccess);
            Console.WriteLine(response.Result.Value);
        }

        [Test]
        public async Task NsfwCheckTestSafe()
        {
            var url = "https://qa.golos.steepshot.org/api/v1/image/9cce1275-3e63-44cd-86db-8359d0128157.jpeg";
            var response = await _api.NsfwCheck(url, CancellationToken.None);
            AssertResult(response);
            Assert.IsTrue(response.IsSuccess);
            Console.WriteLine(response.Result.Value);
            Assert.IsTrue(response.Result.Value <= 0.5);
        }

        [Test]
        public async Task NsfwCheckTestUnsafe()
        {
            var url = "https://qa.golos.steepshot.org/api/v1/image/e827f920-db8b-4718-9d30-aca19b66d190.jpeg";
            var response = await _api.NsfwCheck(url, CancellationToken.None);
            AssertResult(response);
            Assert.IsTrue(response.IsSuccess);
            Console.WriteLine(response.Result.Value);
            Assert.IsTrue(response.Result.Value > 0.5);
        }
    }
}
