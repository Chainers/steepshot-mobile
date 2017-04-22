using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Sweetshot.Library.HttpClient;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;

namespace Sweetshot.Tests.Steemit
{
    [TestFixture]
    public class IntegrationTestsChangingState
    {
        private readonly SteepshotApiClient _api = new SteepshotApiClient(ConfigurationManager.AppSettings["steepshot_url"]);

        [Test]
        public void BlockchainStateChangingTest()
        {
            const string name = "joseph.kalu";
            const string password = "test12345";
            const string newPassword = "test123456";

            var sessionId = Authenticate(name, password);

            // 1) Create new post
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var path = Path.Combine(dir.Parent.Parent.FullName, @"Data/cat.jpg");
            var file = File.ReadAllBytes(path);

            var createPostRequest = new UploadImageRequest(sessionId, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");
            var createPostResponse = _api.Upload(createPostRequest).Result;

            AssertResult(createPostResponse);
            Assert.That(createPostResponse.Result.Body, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Title, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Tags, Is.Not.Empty);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load last created post
            var userPostsRequest = new UserPostsRequest(name);
            var userPostsResponse = _api.GetUserPosts(userPostsRequest).Result;
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First();
            Assert.That(createPostResponse.Result.Title, Is.EqualTo(lastPost.Title));

            // 2) Create new comment
            // Wait for 20 seconds before commenting
            Thread.Sleep(TimeSpan.FromSeconds(20));
            const string body = "Ллойс!";
            const string title = "Лучший камент ever";
            var createCommentRequest = new CreateCommentRequest(sessionId, lastPost.Url, body, title);
            var createCommentResponse = _api.CreateComment(createCommentRequest).Result;
            AssertResult(createCommentResponse);
            Assert.That(createCommentResponse.Result.IsCreated, Is.True);
            Assert.That(createCommentResponse.Result.Message, Is.EqualTo("Comment created"));

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load comments for this post and check them
            var getCommentsRequest = new GetCommentsRequest(lastPost.Url);
            var commentsResponse = _api.GetComments(getCommentsRequest).Result;
            AssertResult(commentsResponse);
            Assert.That(commentsResponse.Result.Results.First().Title, Is.EqualTo(title));
            Assert.That(commentsResponse.Result.Results.First().Body, Is.EqualTo(body));

            // 3) Vote up
            var voteUpRequest = new VoteRequest(sessionId, true, lastPost.Url);
            var voteUpResponse = _api.Vote(voteUpRequest).Result;
            AssertResult(voteUpResponse);
            Assert.That(voteUpResponse.Result.IsVoted, Is.True);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpResponse.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            userPostsRequest.SessionId = sessionId;
            var userPostsResponse2 = _api.GetUserPosts(userPostsRequest).Result;
            // Check if last post was voted
            AssertResult(userPostsResponse2);
            Assert.That(userPostsResponse2.Result.Results.First().Vote, Is.True);

            // 4) Vote down
            var voteDownRequest = new VoteRequest(sessionId, false, lastPost.Url);
            var voteDownResponse = _api.Vote(voteDownRequest).Result;
            AssertResult(voteDownResponse);
            Assert.That(voteDownResponse.Result.IsVoted, Is.False);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownResponse.Result.Message, Is.EqualTo("Downvoted"));
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            userPostsRequest.SessionId = sessionId;
            var userPostsResponse3 = _api.GetUserPosts(userPostsRequest).Result;
            // Check if last post was voted
            AssertResult(userPostsResponse3);
            Assert.That(userPostsResponse3.Result.Results.First().Vote, Is.False);

            // 5) Vote up comment
            var commentUrl = commentsResponse.Result.Results.First().Url.Split('#').Last();
            var voteUpCommentRequest = new VoteRequest(sessionId, true, commentUrl);
            var voteUpCommentResponse = _api.Vote(voteUpCommentRequest).Result;
            AssertResult(voteUpCommentResponse);
            Assert.That(voteUpCommentResponse.Result.IsVoted, Is.True);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpCommentResponse.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.SessionId = sessionId;
            var commentsResponse2 = _api.GetComments(getCommentsRequest).Result;
            // Check if last comment was voted
            AssertResult(commentsResponse2);
            Assert.That(commentsResponse2.Result.Results.First().Vote, Is.True);

            // 6) Vote down comment
            var voteDownCommentRequest = new VoteRequest(sessionId, false, commentUrl);
            var voteDownCommentResponse = _api.Vote(voteDownCommentRequest).Result;
            AssertResult(voteDownCommentResponse);
            Assert.That(voteDownCommentResponse.Result.IsVoted, Is.False);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownCommentResponse.Result.Message, Is.EqualTo("Downvoted"));
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.SessionId = sessionId;
            var commentsResponse3 = _api.GetComments(getCommentsRequest).Result;
            // Check if last comment was voted
            AssertResult(commentsResponse3);
            Assert.That(commentsResponse3.Result.Results.First().Vote, Is.False);

            // 7) Follow
            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            var followRequest = new FollowRequest(sessionId, FollowType.Follow, "asduj");
            var followResponse = _api.Follow(followRequest).Result;
            AssertResult(followResponse);
            Assert.That(followResponse.Result.IsFollowed, Is.True);
            Assert.That(followResponse.Result.Message, Is.EqualTo("User is followed"));

            // 8) UnFollow
            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            var unfollowRequest = new FollowRequest(sessionId, FollowType.UnFollow, "asduj");
            var unfollowResponse = _api.Follow(unfollowRequest).Result;
            AssertResult(unfollowResponse);
            Assert.That(unfollowResponse.Result.IsFollowed, Is.False);
            Assert.That(unfollowResponse.Result.Message, Is.EqualTo("User is unfollowed"));

            // 9) Change password
            var changePasswordRequest = new ChangePasswordRequest(sessionId, password, newPassword);
            var changePasswordResponse = _api.ChangePassword(changePasswordRequest).Result;
            AssertResult(changePasswordResponse);
            Assert.That(changePasswordResponse.Result.IsChanged, Is.True);
            Assert.That(changePasswordResponse.Result.Message, Is.EqualTo("Password was changed"));

            // Rollback
            // New sessionId with new credentials
            var newSessionId = Authenticate(name, newPassword);
            var changePasswordRequest2 = new ChangePasswordRequest(newSessionId, newPassword, password);
            var changePasswordResponse2 = _api.ChangePassword(changePasswordRequest2).Result;
            AssertResult(changePasswordResponse2);
            Assert.That(changePasswordResponse.Result.IsChanged, Is.True);
            Assert.That(changePasswordResponse2.Result.Message, Is.EqualTo("Password was changed"));

            // Update sessionId
            sessionId = Authenticate(name, password);

            // 10) Logout
            var logoutRequest = new LogoutRequest(sessionId);
            var logoutResponse = _api.Logout(logoutRequest).Result;
            AssertResult(logoutResponse);
            Assert.That(logoutResponse.Result.IsLoggedOut, Is.True);
            Assert.That(logoutResponse.Result.Message, Is.EqualTo("User is logged out"));
        }

        private string Authenticate(string name, string password)
        {
            var request = new LoginRequest(name, password);
            var response = _api.Login(request).Result;
            return response.Result.SessionId;
        }

        [Test]
        public void Register_Test()
        {
            const string postingKey = "5JXCxj6YyyGUTJo9434ZrQ5gfxk59rE3yukN42WBA6t58yTPRTG";
            const string name = "joseph.kalu";
            const string password = "test12345";

            // Arrange
            var request = new RegisterRequest(postingKey, name, password);

            // Act
            var response = _api.Register(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.IsLoggedIn, Is.True);
            Assert.That(response.Result.SessionId, Is.Not.Empty);
            Assert.That(response.Result.Message, Is.Not.Empty);
        }

        [Ignore("Ingoring...")]
        public void Upload_Throttling()
        {
            //// Arrange
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\cat.jpg");
            //var file = File.ReadAllBytes(path);
            //var request = new UploadImageRequest(_sessionId, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");

            //// Act
            //var response = _api.Upload(request).Result;
            //var response2 = _api.Upload(request).Result;
            //var response3 = _api.Upload(request).Result;

            //// Assert
            //AssertFailedResult(response3);
            //Assert.That(response3.Errors.Contains("Creating post is impossible. Please try 10 minutes later."));
        }

        private void AssertResult<T>(OperationResult<T> response)
        {
            Assert.That(response, Is.Not.Null);

            if (response.Success)
            {
                Assert.That(response.Result, Is.Not.Null);
                Assert.That(response.Errors, Is.Empty);
            }
            else
            {
                Assert.That(response.Result, Is.Null);
                Assert.That(response.Errors, Is.Not.Empty);

                foreach (var error in response.Errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}
