using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using System.Threading.Tasks;

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
            var request = new AuthorizedRequest(user);
            var response = await Api[apiName].LoginWithPostingKey(request, CancellationToken.None);
            AssertResult(response);
            Assert.That(response.Success, Is.True);
            Assert.That(response.Result.IsSuccess, Is.True);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UploadWithPrepareTest(KnownChains apiName)
        {
            var user = Users[apiName];

            // 1) Create new post
            var file = File.ReadAllBytes(GetTestImagePath());
            user.IsNeedRewards = false;
            var createPostRequest = new UploadImageRequest(user, "cat" + DateTime.UtcNow.Ticks, file, new[] { "cat1", "cat2", "cat3", "cat4" });
            var servResp = await Api[apiName].UploadWithPrepare(createPostRequest, CancellationToken.None);
            AssertResult(servResp);
            var createPostResponse = await Api[apiName].Upload(createPostRequest, servResp.Result, CancellationToken.None);

            AssertResult(createPostResponse);
            Assert.That(createPostResponse.Result.Body, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Title, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Tags, Is.Not.Empty);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load last created post
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var userPostsResponse = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.FirstOrDefault(i => i.Url.EndsWith(createPostResponse.Result.Permlink, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(lastPost);
            Assert.That(createPostResponse.Result.Title, Is.EqualTo(lastPost.Title));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task CreateCommentTest(KnownChains apiName)
        {
            var user = Users[apiName];

            // Load last created post
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var userPostsResponse = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First();

            // 2) Create new comment
            // Wait for 20 seconds before commenting
            Thread.Sleep(TimeSpan.FromSeconds(20));
            const string body = "Ллойс!";
            var createCommentRequest = new CommentRequest(user, lastPost.Url, body, AppSettings.AppInfo);
            var createCommentResponse = await Api[apiName].CreateComment(createCommentRequest, CancellationToken.None);
            AssertResult(createCommentResponse);
            Assert.That(createCommentResponse.Result.IsSuccess, Is.True);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoRequest(lastPost.Url);
            var commentsResponse = await Api[apiName].GetComments(getCommentsRequest, CancellationToken.None);
            AssertResult(commentsResponse);
            Assert.IsNotNull(commentsResponse.Result.Results.FirstOrDefault(i => i.Url.EndsWith(createCommentResponse.Result.Permlink)));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task VotePostTest(KnownChains apiName)
        {
            var user = Users[apiName];

            // Load last created post
            var userPostsRequest = new PostsRequest(PostType.New) { Login = user.Login };
            var userPostsResponse = await Api[apiName].GetPosts(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => !i.Vote);

            // 4) Vote up
            var voteUpRequest = new VoteRequest(user, VoteType.Up, lastPost.Url);
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
            var voteDownRequest = new VoteRequest(user, VoteType.Down, lastPost.Url);
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
            var userPostsRequest = new UserPostsRequest(user.Login) { ShowLowRated = true, ShowNsfw = true };
            var userPostsResponse = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => i.Children > 0);
            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoRequest(lastPost.Url);
            var commentsResponse = await Api[apiName].GetComments(getCommentsRequest, CancellationToken.None);

            // 5) Vote up comment
            var commentUrl = commentsResponse.Result.Results.First().Url.Split('#').Last();
            var voteUpCommentRequest = new VoteRequest(user, VoteType.Up, commentUrl);
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
            var voteDownCommentRequest = new VoteRequest(user, VoteType.Down, commentUrl);
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
            var followRequest = new FollowRequest(user, FollowType.Follow, followUser);
            var followResponse = await Api[apiName].Follow(followRequest, CancellationToken.None);
            AssertResult(followResponse);
            Assert.IsTrue(followResponse.Result.IsSuccess);

            // 8) UnFollow
            var unfollowRequest = new FollowRequest(user, FollowType.UnFollow, followUser);
            var unfollowResponse = await Api[apiName].Follow(unfollowRequest, CancellationToken.None);
            AssertResult(unfollowResponse);
            Assert.IsTrue(unfollowResponse.Result.IsSuccess);
        }
    }
}
