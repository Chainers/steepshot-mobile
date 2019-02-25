using System;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Models;
using MediaUpload.Tests.Models;
using NUnit.Framework;

namespace MediaUpload.Tests
{
    [TestFixture]
    public class UploadMediaTest : BaseTests
    {
        [Test]
        [TestCase(AuthType.Steem)]
        [TestCase(AuthType.Golos)]
        public async Task UploadSucces(AuthType value)
        {
            TokenModel tokenModel;
            switch (value)
            {
                case AuthType.Steem:
                    tokenModel = await AuthorizeToSteem();
                    break;
                case AuthType.Golos:
                    tokenModel = await AuthorizeToGolos();
                    break;
                default:
                    throw new NotImplementedException();
            }

            var model = new MediaModel
            {
                Aws = true,
                Ipfs = true,
                Thumbnails = true
            };

            var servResp = await UploadMediaAsync(model, GetTestImagePath("cat.jpg"), tokenModel, CancellationToken.None);
            AssertResult(servResp);
        }
    }
}
