using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using System.Threading.Tasks;
using Steepshot.Core.Models.Enums;
using Ditch.Core.Helpers;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Tests.HttpClient
{
    [TestFixture]
    public class ApiTest : BaseTests
    {
        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task LoginWithPostingKeyTest(KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new AuthorizedModel(user);
            var response = await Api[apiName].LoginWithPostingKey(request, CancellationToken.None);
            AssertResult(response);
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Result.IsSuccess, Is.True);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task UploadMediaTest(KnownChains apiName)
        {
            var user = Users[apiName];

            // 1) Create new post
            var path = GetTestImagePath();
            var stream = new FileStream(GetTestImagePath(), FileMode.Open);
            user.IsNeedRewards = false;
            var uploadImageModel = new UploadMediaModel(user, stream, Path.GetExtension(path));
            var servResp = await Api[apiName].UploadMedia(uploadImageModel, CancellationToken.None);
            AssertResult(servResp);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task PreparePostTest(KnownChains apiName)
        {
            var user = Users[apiName];
            var model = new PreparePostModel(user)
            {
                Title = "Test",
                Description = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Media = new[]
                {
                    new MediaModel
                    {
                        Url = "http://steepshot.org/api/v1/image/034e7cc2-90df-4186-b475-9b7d4166e0a4.jpeg",
                        IpfsHash = "QmUHaQDMc46pR21fNFt1Gxo5YeeFxD4uENywbevXe5XXWM",
                        Size = new FrameSize
                        {
                            Height = 194,
                            Width = 194
                        }
                    }
                },
                Tags = new[] { "test" }
            };

            var createPostResponse = await Api[apiName].PreparePost(model, CancellationToken.None);
            AssertResult(createPostResponse);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task CreateCommentTest(KnownChains apiName)
        {
            var user = Users[apiName];

            // Load last created post
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var userPostsResponse = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First();

            // 2) Create new comment
            // Wait for 20 seconds before commenting
            Thread.Sleep(TimeSpan.FromSeconds(20));
            var createCommentModel = new CreateCommentModel(user, lastPost.Url, $"Test comment {DateTime.Now:G}", AppSettings.AppInfo);
            var createCommentResponse = await Api[apiName].CreateComment(createCommentModel, CancellationToken.None);
            AssertResult(createCommentResponse);
            Assert.That(createCommentResponse.Result.IsSuccess, Is.True);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoModel(lastPost.Url);
            var commentsResponse = await Api[apiName].GetComments(getCommentsRequest, CancellationToken.None);
            AssertResult(commentsResponse);

            UrlHelper.TryCastUrlToAuthorAndPermlink(createCommentModel.ParentUrl, out var parentAuthor, out var parentPermlink);
            var permlink = OperationHelper.CreateReplyPermlink(user.Login, parentAuthor, parentPermlink);
            Assert.IsNotNull(commentsResponse.Result.Results.FirstOrDefault(i => i.Url.EndsWith(permlink)));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task VotePostTest(KnownChains apiName)
        {
            var user = Users[apiName];

            // Load last created post
            var userPostsRequest = new PostsModel(PostType.New) { Login = user.Login };
            var userPostsResponse = await Api[apiName].GetPosts(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => !i.Vote);

            // 4) Vote up
            var voteUpRequest = new VoteModel(user, VoteType.Up, lastPost.Url);
            var voteUpResponse = await Api[apiName].Vote(voteUpRequest, CancellationToken.None);
            AssertResult(voteUpResponse);
            Assert.That(voteUpResponse.Result.IsSuccess, Is.True);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            //Assert.IsTrue(lastPost.TotalPayoutReward <= voteUpResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            userPostsRequest.Offset = lastPost.Url;
            var userPostsResponse2 = await Api[apiName].GetPosts(userPostsRequest, CancellationToken.None);
            // Check if last post was voted
            AssertResult(userPostsResponse2);
            var post = userPostsResponse2.Result.Results.FirstOrDefault(i => i.Url.EndsWith(lastPost.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(post);
            Console.WriteLine("The server still updates the history");
            //Assert.That(post.Vote, Is.True);

            // 3) Vote down
            var voteDownRequest = new VoteModel(user, VoteType.Down, lastPost.Url);
            var voteDownResponse = await Api[apiName].Vote(voteDownRequest, CancellationToken.None);
            AssertResult(voteDownResponse);
            Assert.That(voteDownResponse.Result.IsSuccess, Is.True);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            //Assert.IsTrue(lastPost.TotalPayoutReward >= voteDownResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            var userPostsResponse3 = await Api[apiName].GetPosts(userPostsRequest, CancellationToken.None);
            // Check if last post was voted
            AssertResult(userPostsResponse3);
            post = userPostsResponse3.Result.Results.FirstOrDefault(i => i.Url.Equals(lastPost.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(post);
            Console.WriteLine("The server still updates the history");
            //Assert.That(post.Vote, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task VoteCommentTest(KnownChains apiName)
        {
            var user = Users[apiName];

            // Load last created post
            var userPostsRequest = new UserPostsModel(user.Login) { ShowLowRated = true, ShowNsfw = true };
            var userPostsResponse = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => i.Children > 0);
            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoModel(lastPost.Url);
            var commentsResponse = await Api[apiName].GetComments(getCommentsRequest, CancellationToken.None);

            // 5) Vote up comment
            var commentUrl = commentsResponse.Result.Results.First().Url.Split('#').Last();
            var voteUpCommentRequest = new VoteModel(user, VoteType.Up, commentUrl);
            var voteUpCommentResponse = await Api[apiName].Vote(voteUpCommentRequest, CancellationToken.None);
            AssertResult(voteUpCommentResponse);
            Assert.That(voteUpCommentResponse.Result.IsSuccess, Is.True);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            getCommentsRequest.Login = user.Login;
            var commentsResponse2 = await Api[apiName].GetComments(getCommentsRequest, CancellationToken.None);
            // Check if last comment was voted
            AssertResult(commentsResponse2);
            var comm = commentsResponse2.Result.Results.FirstOrDefault(i => i.Url.EndsWith(commentUrl, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(comm);
            Assert.That(comm.Vote, Is.True);

            // 6) Vote down comment
            var voteDownCommentRequest = new VoteModel(user, VoteType.Down, commentUrl);
            var voteDownCommentResponse = await Api[apiName].Vote(voteDownCommentRequest, CancellationToken.None);
            AssertResult(voteDownCommentResponse);
            Assert.That(voteDownCommentResponse.Result.IsSuccess, Is.True);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            getCommentsRequest.Login = user.Login;
            var commentsResponse3 = await Api[apiName].GetComments(getCommentsRequest, CancellationToken.None);
            // Check if last comment was voted
            AssertResult(commentsResponse3);
            comm = commentsResponse3.Result.Results.FirstOrDefault(i => i.Url.EndsWith(commentUrl, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(comm);
            Assert.That(comm.Vote, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem, "asduj")]
        [TestCase(KnownChains.Golos, "pmartynov")]
        public async Task FollowTest(KnownChains apiName, string followUser)
        {
            var user = Users[apiName];

            // 7) Follow
            var followRequest = new FollowModel(user, FollowType.Follow, followUser);
            var followResponse = await Api[apiName].Follow(followRequest, CancellationToken.None);
            AssertResult(followResponse);
            Assert.IsTrue(followResponse.Result.IsSuccess);

            // 8) UnFollow
            var unfollowRequest = new FollowModel(user, FollowType.UnFollow, followUser);
            var unfollowResponse = await Api[apiName].Follow(unfollowRequest, CancellationToken.None);
            AssertResult(unfollowResponse);
            Assert.IsTrue(unfollowResponse.Result.IsSuccess);
        }


        [Test]
        [TestCase(KnownChains.Steem, "joseph.kalu")]
        [TestCase(KnownChains.Golos, "joseph.kalu")]
        public async Task UpdateUserProfileTest(KnownChains apiName, string followUser)
        {
            var user = Users[apiName];

            var userProfileModel = new UserProfileModel(user.Login);
            var profileResponse = await Api[apiName].GetUserProfile(userProfileModel, CancellationToken.None);
            AssertResult(profileResponse);
            Assert.IsTrue(profileResponse.IsSuccess);
            var profile = profileResponse.Result;

            var updateUserProfileModel = new UpdateUserProfileModel()
            {
                Login = user.Login,
                ActiveKey = user.PostingKey,
                About = profile.About,
                Location = profile.Location,
                Name = profile.Name,
                ProfileImage = profile.ProfileImage,
                Website = profile.Website
            };
            var responce = await Api[apiName].UpdateUserProfile(updateUserProfileModel, CancellationToken.None);
            AssertResult(responce);
            Assert.IsTrue(responce.Result.IsSuccess);
        }
    }
}
