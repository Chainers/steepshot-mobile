using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Serializing;
using Steepshot.Core.Tests.Stubs;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class ServerResponseTests
    {
        private const bool IsDev = false;
        private static readonly Dictionary<KnownChains, UserInfo> Users;
        private static readonly Dictionary<KnownChains, BaseServerClient> Gateway;

        static ServerResponseTests()
        {
            var converter = new JsonNetConverter();
            Gateway = new Dictionary<KnownChains, BaseServerClient>
            {
                {KnownChains.Steem, new StubServerClient(converter, IsDev ? Constants.SteemUrlQa : Constants.SteemUrl)},
                {KnownChains.Golos, new StubServerClient(converter, IsDev ? Constants.GolosUrlQa : Constants.GolosUrl)},
            };

            Users = new Dictionary<KnownChains, UserInfo>
            {
                {KnownChains.Steem, new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["SteemWif"]}},
                {KnownChains.Golos, new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["GolosWif"]}},
            };
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task GetUserPostsTest(KnownChains apiName)
        {
            var user = Users[apiName];

            var request = new UserPostsModel(user.Login)
            {
                ShowNsfw = true,
                ShowLowRated = true
            };

            var result = await Gateway[apiName].GetUserPosts(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task GetUserRecentPostsTest(KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new CensoredNamedRequestWithOffsetLimitModel
            {
                Login = user.Login,
                ShowLowRated = true,
                ShowNsfw = true
            };

            var result = await Gateway[apiName].GetUserRecentPosts(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task GetPostsTest(KnownChains apiName)
        {
            var request = new PostsModel(PostType.Top);

            var result = await Gateway[apiName].GetPosts(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem, "food")]
        [TestCase(KnownChains.Golos, "ru--golos")]
        public async Task GetPostsByCategoryTest(KnownChains apiName, string category)
        {
            var request = new PostsByCategoryModel(PostType.Top, category);
            var result = await Gateway[apiName].GetPostsByCategory(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem, "@steepshot/steepshot-some-stats-and-explanations")]
        [TestCase(KnownChains.Golos, "@irina1/avto-2018-01-04-10-43-52")]
        public async Task GetPostVotersTest(KnownChains apiName, string url)
        {
            var request = new VotersModel(url, VotersType.All)
            {
                Limit = 40,
                Offset = string.Empty,

            };

            var result = await Gateway[apiName].GetPostVoters(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem, "@joseph.kalu/cat636203355240074655")]
        [TestCase(KnownChains.Golos, "@joseph.kalu/cat636281384922864910")]
        public async Task GetCommentsTest(KnownChains apiName, string url)
        {
            var request = new NamedInfoModel(url);
            var result = await Gateway[apiName].GetComments(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task GetUserProfileTest(KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserProfileModel(user.Login);
            var result = await Gateway[apiName].GetUserProfile(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }


        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task GetUserFriendsTest(KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserFriendsModel(user.Login, FriendsType.Following);
            var result = await Gateway[apiName].GetUserFriends(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem, "/steepshot/@joseph.kalu/cat636416737569422613-2017-09-22-10-42-38")]
        [TestCase(KnownChains.Golos, "/steepshot/@joseph.kalu/cat636416737747907631-2017-09-22-10-42-56")]
        public async Task GetPostInfoTest(KnownChains apiName, [Values()] string url)
        {
            var request = new NamedInfoModel(url)
            {
                ShowNsfw = true,
                ShowLowRated = true
            };
            var result = await Gateway[apiName].GetPostInfo(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task SearchUserTest(KnownChains apiName)
        {
            var request = new SearchWithQueryModel("aar");
            var result = await Gateway[apiName].SearchUser(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserExistsCheckTest(KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserExistsModel(user.Login);
            var result = await Gateway[apiName].UserExistsCheck(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task GetCategoriesTest(KnownChains apiName)
        {
            var request = new OffsetLimitModel();
            var result = await Gateway[apiName].GetCategories(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task SearchCategoriesTest(KnownChains apiName)
        {
            var request = new SearchWithQueryModel("ru");
            var result = await Gateway[apiName].SearchCategories(request, CancellationToken.None);
            Assert.IsTrue(result.IsSuccess, result.Error?.Message);
        }
    }
}
