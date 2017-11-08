using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests.HttpClient
{
    [TestFixture]
    public class ApiTest : BaseTests
    {
        [Test, Sequential]
        public void LoginWithPostingKeyTest([Values("Steem", "Golos")] string apiName)
        {
            var api = Api[apiName];
            var user = Users[apiName];
            var request = new AuthorizedRequest(user);
            var response = api.LoginWithPostingKey(request, CancellationToken.None).Result;
            AssertResult(response);
            Assert.That(response.Result.IsLoggedIn, Is.True);
        }

        [Test, Sequential]
        public void UploadWithPrepareTest([Values("Steem", "Golos")] string apiName)
        {
            var user = Authenticate(apiName);

            // 1) Create new post
            var file = File.ReadAllBytes(GetTestImagePath());
            user.IsNeedRewards = false;
            var createPostRequest = new UploadImageRequest(user, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");
            var servResp = Api[apiName].UploadWithPrepare(createPostRequest, CancellationToken.None).Result;
            AssertResult(servResp);
            var createPostResponse = Api[apiName].Upload(createPostRequest, servResp.Result, CancellationToken.None).Result;

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
            var userPostsResponse = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result;
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.FirstOrDefault(i => i.Url.EndsWith(createPostResponse.Result.Permlink, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(lastPost);
            Assert.That(createPostResponse.Result.Title, Is.EqualTo(lastPost.Title));
        }

        [Test, Sequential]
        public void CreateCommentTest([Values("Steem", "Golos")] string apiName)
        {
            var user = Authenticate(apiName);

            // Load last created post
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var userPostsResponse = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result;
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First();

            // 2) Create new comment
            // Wait for 20 seconds before commenting
            Thread.Sleep(TimeSpan.FromSeconds(20));
            const string body = "Ллойс!";
            var createCommentRequest = new CreateCommentRequest(user, lastPost.Url, body, AppSettings.AppInfo);
            var createCommentResponse = Api[apiName].CreateComment(createCommentRequest, CancellationToken.None).Result;
            AssertResult(createCommentResponse);
            Assert.That(createCommentResponse.Result.IsCreated, Is.True);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoRequest(lastPost.Url);
            var commentsResponse = Api[apiName].GetComments(getCommentsRequest, CancellationToken.None).Result;
            AssertResult(commentsResponse);
            Assert.IsNotNull(commentsResponse.Result.Results.FirstOrDefault(i => i.Url.EndsWith(createCommentResponse.Result.Permlink)));
        }

        [Test, Sequential]
        public void VotePostTest([Values("Steem", "Golos")] string apiName)
        {
            var user = Authenticate(apiName);

            // Load last created post
            var userPostsRequest = new PostsRequest(PostType.New) { Login = user.Login };
            var userPostsResponse = Api[apiName].GetPosts(userPostsRequest, CancellationToken.None).Result;
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => !i.Vote);

            // 4) Vote up
            var voteUpRequest = new VoteRequest(user, VoteType.Up, lastPost.Url);
            var voteUpResponse = Api[apiName].Vote(voteUpRequest, CancellationToken.None).Result;
            AssertResult(voteUpResponse);
            Assert.That(voteUpResponse.Result.IsSucces, Is.True);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            //Assert.IsTrue(lastPost.TotalPayoutReward <= voteUpResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            userPostsRequest.Offset = lastPost.Url;
            var userPostsResponse2 = Api[apiName].GetPosts(userPostsRequest, CancellationToken.None).Result;
            // Check if last post was voted
            AssertResult(userPostsResponse2);
            var post = userPostsResponse2.Result.Results.FirstOrDefault(i => i.Url.EndsWith(lastPost.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(post);
            Console.WriteLine("The server still updates the history");
            //Assert.That(post.Vote, Is.True);

            // 3) Vote down
            var voteDownRequest = new VoteRequest(user, VoteType.Down, lastPost.Url);
            var voteDownResponse = Api[apiName].Vote(voteDownRequest, CancellationToken.None).Result;
            AssertResult(voteDownResponse);
            Assert.That(voteDownResponse.Result.IsSucces, Is.True);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            //Assert.IsTrue(lastPost.TotalPayoutReward >= voteDownResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            var userPostsResponse3 = Api[apiName].GetPosts(userPostsRequest, CancellationToken.None).Result;
            // Check if last post was voted
            AssertResult(userPostsResponse3);
            post = userPostsResponse3.Result.Results.FirstOrDefault(i => i.Url.Equals(lastPost.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(post);
            Console.WriteLine("The server still updates the history");
            //Assert.That(post.Vote, Is.False);
        }

        [Test, Sequential]
        public void VoteCommentTest([Values("Steem", "Golos")] string apiName)
        {
            var user = Authenticate(apiName);

            // Load last created post
            var userPostsRequest = new UserPostsRequest(user.Login) { ShowLowRated = true, ShowNsfw = true };
            var userPostsResponse = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result;
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => i.Children > 0);
            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoRequest(lastPost.Url);
            var commentsResponse = Api[apiName].GetComments(getCommentsRequest, CancellationToken.None).Result;

            // 5) Vote up comment
            var commentUrl = commentsResponse.Result.Results.First().Url.Split('#').Last();
            var voteUpCommentRequest = new VoteRequest(user, VoteType.Up, commentUrl);
            var voteUpCommentResponse = Api[apiName].Vote(voteUpCommentRequest, CancellationToken.None).Result;
            AssertResult(voteUpCommentResponse);
            Assert.That(voteUpCommentResponse.Result.IsSucces, Is.True);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.Login = user.Login;
            var commentsResponse2 = Api[apiName].GetComments(getCommentsRequest, CancellationToken.None).Result;
            // Check if last comment was voted
            AssertResult(commentsResponse2);
            var comm = commentsResponse2.Result.Results.FirstOrDefault(i => i.Url.EndsWith(commentUrl, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(comm);
            Assert.That(comm.Vote, Is.True);

            // 6) Vote down comment
            var voteDownCommentRequest = new VoteRequest(user, VoteType.Down, commentUrl);
            var voteDownCommentResponse = Api[apiName].Vote(voteDownCommentRequest, CancellationToken.None).Result;
            AssertResult(voteDownCommentResponse);
            Assert.That(voteDownCommentResponse.Result.IsSucces, Is.True);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.Login = user.Login;
            var commentsResponse3 = Api[apiName].GetComments(getCommentsRequest, CancellationToken.None).Result;
            // Check if last comment was voted
            AssertResult(commentsResponse3);
            comm = commentsResponse3.Result.Results.FirstOrDefault(i => i.Url.EndsWith(commentUrl, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(comm);
            Assert.That(comm.Vote, Is.False);
        }

        [Test, Sequential]
        public void FollowTest([Values("Steem", "Golos")] string apiName, [Values("asduj", "pmartynov")] string followUser)
        {
            var user = Authenticate(apiName);

            // 7) Follow
            var followRequest = new FollowRequest(user, FollowType.Follow, followUser);
            var followResponse = Api[apiName].Follow(followRequest, CancellationToken.None).Result;
            AssertResult(followResponse);
            Assert.IsTrue(followResponse.Result.IsSuccess);

            // 8) UnFollow
            var unfollowRequest = new FollowRequest(user, FollowType.UnFollow, followUser);
            var unfollowResponse = Api[apiName].Follow(unfollowRequest, CancellationToken.None).Result;
            AssertResult(unfollowResponse);
            Assert.IsTrue(unfollowResponse.Result.IsSuccess);
        }
    }
}
