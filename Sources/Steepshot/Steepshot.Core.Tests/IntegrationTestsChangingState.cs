using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class IntegrationTestsChangingState : BaseTests
    {

        [Test, Sequential]
        public void BlockchainStateChangingTest([Values("Steem", "Golos")] string apiName, [Values("asduj", "pmartynov")] string followUser)
        {
            var user = Authenticate(apiName);

            // 1) Create new post
            var file = File.ReadAllBytes(GetTestImagePath());

            var createPostRequest = new UploadImageRequest(user, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");
            var createPostResponse = Api[apiName].Upload(createPostRequest, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None)).Result;

            AssertResult(createPostResponse);
            Assert.That(createPostResponse.Result.Body, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Title, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Tags, Is.Not.Empty);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load last created post
            var userPostsRequest = new UserPostsRequest(user.Login);
            var userPostsResponse = Api[apiName].GetUserPosts(userPostsRequest).Result;
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First();
            Assert.That(createPostResponse.Result.Title, Is.EqualTo(lastPost.Title));

            // 2) Create new comment
            // Wait for 20 seconds before commenting
            Thread.Sleep(TimeSpan.FromSeconds(20));
            const string body = "Ллойс!";
            const string title = "Лучший камент ever";
            var createCommentRequest = new CreateCommentRequest(user, lastPost.Url, body, title, AppSettings.AppInfo);
            var createCommentResponse = Api[apiName].CreateComment(createCommentRequest).Result;
            AssertResult(createCommentResponse);
            Assert.That(createCommentResponse.Result.IsCreated, Is.True);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoRequest(lastPost.Url);
            var commentsResponse = Api[apiName].GetComments(getCommentsRequest).Result;
            AssertResult(commentsResponse);
            Assert.That(commentsResponse.Result.Results.First().Title, Is.EqualTo(title));
            Assert.That(commentsResponse.Result.Results.First().Body, Is.EqualTo(body));

            // 3) Vote down
            var voteDownRequest = new VoteRequest(user, VoteType.Down, lastPost.Url);
            var voteDownResponse = Api[apiName].Vote(voteDownRequest).Result;
            AssertResult(voteDownResponse);
            Assert.That(voteDownResponse.Result.IsSucces, Is.False);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.IsTrue(lastPost.TotalPayoutReward <= voteDownResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            userPostsRequest.Login = user.Login;
            var userPostsResponse3 = Api[apiName].GetUserPosts(userPostsRequest).Result;
            // Check if last post was voted
            AssertResult(userPostsResponse3);
            Assert.That(userPostsResponse3.Result.Results.First().Vote, Is.False);

            // 4) Vote up
            var voteUpRequest = new VoteRequest(user, VoteType.Up, lastPost.Url);
            var voteUpResponse = Api[apiName].Vote(voteUpRequest).Result;
            AssertResult(voteUpResponse);
            Assert.That(voteUpResponse.Result.IsSucces, Is.True);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.IsTrue(lastPost.TotalPayoutReward <= voteUpResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            userPostsRequest.Login = user.Login;
            var userPostsResponse2 = Api[apiName].GetUserPosts(userPostsRequest).Result;
            // Check if last post was voted
            AssertResult(userPostsResponse2);
            Assert.That(userPostsResponse2.Result.Results.First().Vote, Is.True);

            // 5) Vote up comment
            var commentUrl = commentsResponse.Result.Results.First().Url.Split('#').Last();
            var voteUpCommentRequest = new VoteRequest(user, VoteType.Up, commentUrl);
            var voteUpCommentResponse = Api[apiName].Vote(voteUpCommentRequest).Result;
            AssertResult(voteUpCommentResponse);
            Assert.That(voteUpCommentResponse.Result.IsSucces, Is.True);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.Login = user.Login;
            var commentsResponse2 = Api[apiName].GetComments(getCommentsRequest).Result;
            // Check if last comment was voted
            AssertResult(commentsResponse2);
            Assert.That(commentsResponse2.Result.Results.First().Vote, Is.True);

            // 6) Vote down comment
            var voteDownCommentRequest = new VoteRequest(user, VoteType.Down, commentUrl);
            var voteDownCommentResponse = Api[apiName].Vote(voteDownCommentRequest).Result;
            AssertResult(voteDownCommentResponse);
            Assert.That(voteDownCommentResponse.Result.IsSucces, Is.False);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.Login = user.Login;
            var commentsResponse3 = Api[apiName].GetComments(getCommentsRequest).Result;
            // Check if last comment was voted
            AssertResult(commentsResponse3);
            Assert.That(commentsResponse3.Result.Results.First().Vote, Is.False);

            // 7) Follow
            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            var followRequest = new FollowRequest(user, FollowType.Follow, followUser);
            var followResponse = Api[apiName].Follow(followRequest).Result;
            AssertResult(followResponse);
            Assert.That(followResponse.Result.IsSuccess, Is.True);

            // 8) UnFollow
            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            var unfollowRequest = new FollowRequest(user, FollowType.UnFollow, followUser);
            var unfollowResponse = Api[apiName].Follow(unfollowRequest).Result;
            AssertResult(unfollowResponse);
            Assert.That(unfollowResponse.Result.IsSuccess, Is.False);

            // 9) Logout
            var logoutRequest = new AuthorizedRequest(user);
            var logoutResponse = Api[apiName].Logout(logoutRequest).Result;
            AssertResult(logoutResponse);
            Assert.That(logoutResponse.Result.IsLoggedOut, Is.True);
        }
    }
}