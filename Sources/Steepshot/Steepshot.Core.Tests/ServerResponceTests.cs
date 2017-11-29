using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Authority;
using Steepshot.Core.Serializing;
using Steepshot.Core.Tests.Stubs;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class ServerResponceTests
    {
        private const bool IsDev = false;
        private static readonly Dictionary<KnownChains, UserInfo> Users;
        private static readonly Dictionary<KnownChains, BaseServerClient> Gateway;

        static ServerResponceTests()
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
        public async Task GetUserPostsTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var user = Users[apiName];

            var request = new UserPostsRequest(user.Login)
            {
                ShowNsfw = true,
                ShowLowRated = true
            };

            var errors = await Gateway[apiName].GetUserPosts(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test]
        public async Task GetUserRecentPostsTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new CensoredNamedRequestWithOffsetLimitFields
            {
                Login = user.Login,
                ShowLowRated = true,
                ShowNsfw = true
            };

            var errors = await Gateway[apiName].GetUserRecentPosts(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test]
        public async Task GetPostsTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var request = new PostsRequest(PostType.Top);

            var errors = await Gateway[apiName].GetPosts(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test, Sequential]
        public async Task GetPostsByCategoryTest(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("food", "ru--golos")] string category)
        {
            var request = new PostsByCategoryRequest(PostType.Top, category);
            var errors = await Gateway[apiName].GetPostsByCategory(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test, Sequential]
        public async Task GetPostVotersTest(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("@steepshot/steepshot-some-stats-and-explanations", "@anatolich/utro-dobroe-gospoda-i-damy-khochu-chtoby-opyatx-bylo-leto-plyazh-i-solncze--2017-11-08-02-10-33")] string url)
        {
            var request = new VotersRequest(url, VotersType.All)
            {
                Limit = 40,
                Offset = string.Empty,

            };

            var errors = await Gateway[apiName].GetPostVoters(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test, Sequential]
        public async Task GetCommentsTest(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("@joseph.kalu/cat636203355240074655", "@joseph.kalu/cat636281384922864910")] string url)
        {
            var request = new NamedInfoRequest(url);
            var errors = await Gateway[apiName].GetComments(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test]
        public async Task GetUserProfileTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserProfileRequest(user.Login);
            var errors = await Gateway[apiName].GetUserProfile(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }


        [Test]
        public async Task GetUserFriendsTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserFriendsRequest(user.Login, FriendsType.Following);
            var errors = await Gateway[apiName].GetUserFriends(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test, Sequential]
        public async Task GetPostInfoTest(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("/steepshot/@joseph.kalu/cat636416737569422613-2017-09-22-10-42-38", "/steepshot/@joseph.kalu/cat636416737747907631-2017-09-22-10-42-56")] string url)
        {
            var request = new NamedInfoRequest(url)
            {
                ShowNsfw = true,
                ShowLowRated = true
            };
            var errors = await Gateway[apiName].GetPostInfo(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test]
        public async Task SearchUserTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var request = new SearchWithQueryRequest("aar");
            var errors = await Gateway[apiName].SearchUser(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test]
        public async Task UserExistsCheckTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserExistsRequests(user.Login);
            var errors = await Gateway[apiName].UserExistsCheck(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test]
        public async Task GetCategoriesTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var request = new OffsetLimitFields();
            var errors = await Gateway[apiName].GetCategories(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }

        [Test]
        public async Task SearchCategoriesTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var request = new SearchWithQueryRequest("ru");
            var errors = await Gateway[apiName].SearchCategories(request, CancellationToken.None);
            Assert.IsTrue(errors.Success, string.Join(Environment.NewLine, errors.Errors));
        }
    }
}
