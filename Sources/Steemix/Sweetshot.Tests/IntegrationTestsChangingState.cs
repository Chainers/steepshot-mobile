using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Sweetshot.Library.HttpClient;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;

namespace Sweetshot.Tests
{
    [TestFixture]
    public class IntegrationTestsChangingState
    {
        private const string Name = "joseph.kalu";
        private const string Password = "test12345";
        private const string NewPassword = "test123456";
        private string _sessionId = string.Empty;

        private readonly SteepshotApiClient _api = new SteepshotApiClient(ConfigurationManager.AppSettings["sweetshot_url"]);

        [SetUp]
        public void Authenticate()
        {
            var request = new LoginRequest(Name, Password);
            _sessionId = _api.Login(request).Result.Result.SessionId;
        }

        [Test]
        public void BlockchainStateChangingTest()
        {
            // 1) Create new post
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var path = Path.Combine(dir.Parent.Parent.FullName, @"Data/cat.jpg");
            var file = File.ReadAllBytes(path);
            
            var createPostRequest = new UploadImageRequest(_sessionId, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");
            var createPostResponse = _api.Upload(createPostRequest).Result;

            AssertSuccessfulResult(createPostResponse);
            Assert.That(createPostResponse.Result.Body, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Title, Is.Not.Empty);
            Assert.That(createPostResponse.Result.Tags, Is.Not.Empty);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load last created post
            var userPostsRequest = new UserPostsRequest(Name);
            var lastPost = _api.GetUserPosts(userPostsRequest).Result.Result.Results.First();
            Assert.That(createPostResponse.Result.Title, Is.EqualTo(lastPost.Title));

            // 2) Create new comment
            const string body = "Ллойс!";
            const string title = "Лучший камент ever";
            var createCommentRequest = new CreateCommentRequest(_sessionId, lastPost.Url, body, title);
            var createCommentResponse = _api.CreateComment(createCommentRequest).Result;

            AssertSuccessfulResult(createCommentResponse);
            Assert.That(createCommentResponse.Result.IsCreated, Is.True);
            Assert.That(createCommentResponse.Result.Message, Is.EqualTo("Comment created"));

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load comments for this post and check them
            var getCommentsRequest = new GetCommentsRequest(lastPost.Url);
            var commentsResponse = _api.GetComments(getCommentsRequest).Result;
            Assert.That(commentsResponse.Result.Results.First().Title, Is.EqualTo(title));
            Assert.That(commentsResponse.Result.Results.First().Body, Is.EqualTo(body));

            // 3) Vote up
            var voteUpRequest = new VoteRequest(_sessionId, true, lastPost.Url);
            var voteUpResponse = _api.Vote(voteUpRequest).Result;

            // Assert
            AssertSuccessfulResult(voteUpResponse);
            Assert.That(voteUpResponse.Result.IsVoted, Is.True);
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpResponse.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(voteUpResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            userPostsRequest.SessionId = _sessionId;
            var userPostsResponse = _api.GetUserPosts(userPostsRequest).Result;
            // Check if last post was voted
            Assert.That(userPostsResponse.Result.Results.First().Vote, Is.True);

            // 4) Vote down
            var voteDownRequest = new VoteRequest(_sessionId, false, lastPost.Url);
            var voteDownResponse = _api.Vote(voteDownRequest).Result;

            AssertSuccessfulResult(voteDownResponse);
            Assert.That(voteDownResponse.Result.IsVoted, Is.False);
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownResponse.Result.Message, Is.EqualTo("Downvoted"));
            Assert.That(voteDownResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            userPostsRequest.SessionId = _sessionId;
            var userPostsResponse2 = _api.GetUserPosts(userPostsRequest).Result;
            // Check if last post was voted
            Assert.That(userPostsResponse2.Result.Results.First().Vote, Is.False);

            // 5) Vote up comment
            var commentUrl = commentsResponse.Result.Results.First().Url.Split('#').Last();
            var voteUpCommentRequest = new VoteRequest(_sessionId, true, commentUrl);
            var voteUpCommentResponse = _api.Vote(voteUpCommentRequest).Result;

            AssertSuccessfulResult(voteUpCommentResponse);
            Assert.That(voteUpCommentResponse.Result.IsVoted, Is.True);
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteUpCommentResponse.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(voteUpCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.SessionId = _sessionId;
            var commentsResponse2 = _api.GetComments(getCommentsRequest).Result;
            // Check if last comment was voted
            Assert.That(commentsResponse2.Result.Results.First().Vote, Is.True);

            // 6) Vote down comment
            var voteDownCommentRequest = new VoteRequest(_sessionId, false, commentUrl);
            var voteDownCommentResponse = _api.Vote(voteDownCommentRequest).Result;

            AssertSuccessfulResult(voteDownCommentResponse);
            Assert.That(voteDownCommentResponse.Result.IsVoted, Is.False);
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(voteDownCommentResponse.Result.Message, Is.EqualTo("Downvoted"));
            Assert.That(voteDownCommentResponse.Result.NewTotalPayoutReward, Is.Not.Null);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Provide sessionId with request to be able read voting information
            getCommentsRequest.SessionId = _sessionId;
            var commentsResponse3 = _api.GetComments(getCommentsRequest).Result;
            // Check if last comment was voted
            Assert.That(commentsResponse3.Result.Results.First().Vote, Is.False);

            // 7) Follow
            var followRequest = new FollowRequest(_sessionId, FollowType.Follow, "asduj");
            var followResponse = _api.Follow(followRequest).Result;

            AssertSuccessfulResult(followResponse);
            Assert.That(followResponse.Result.IsFollowed, Is.True);
            Assert.That(followResponse.Result.Message, Is.EqualTo("User is followed"));

            // 8) UnFollow
            var unfollowRequest = new FollowRequest(_sessionId, FollowType.UnFollow, "asduj");
            var unfollowResponse = _api.Follow(unfollowRequest).Result;

            AssertSuccessfulResult(unfollowResponse);
            Assert.That(unfollowResponse.Result.IsFollowed, Is.False);
            Assert.That(unfollowResponse.Result.Message, Is.EqualTo("User is unfollowed"));

            // 9) Change password
            var changePasswordRequest = new ChangePasswordRequest(_sessionId, Password, NewPassword);
            var changePasswordResponse = _api.ChangePassword(changePasswordRequest).Result;

            AssertSuccessfulResult(changePasswordResponse);
            Assert.That(changePasswordResponse.Result.IsChanged, Is.True);
            Assert.That(changePasswordResponse.Result.Message, Is.EqualTo("Password was changed"));

            // Rollback
            // TODO Refactor it.
            var loginRequest = new LoginRequest(Name, NewPassword);
            var loginResponse = _api.Login(loginRequest).Result;
            var changePasswordRequest2 = new ChangePasswordRequest(loginResponse.Result.SessionId, NewPassword, Password);
            var changePasswordResponse2 = _api.ChangePassword(changePasswordRequest2).Result;
            AssertSuccessfulResult(changePasswordResponse2);
            Assert.That(changePasswordResponse.Result.IsChanged, Is.True);
            Assert.That(changePasswordResponse2.Result.Message, Is.EqualTo("Password was changed"));
            Authenticate();

            // 10) Logout
            var logoutRequest = new LogoutRequest(_sessionId);
            var logoutResponse = _api.Logout(logoutRequest).Result;

            AssertSuccessfulResult(logoutResponse);
            Assert.That(logoutResponse.Result.IsLoggedOut, Is.True);
            Assert.That(logoutResponse.Result.Message, Is.EqualTo("User is logged out"));
        }

        [Ignore("Ignoring")]
        public void Register()
        {
            // Arrange
            var request = new RegisterRequest("", "", "");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            //Assert.That(response.Result.IsLoggedIn, Is.False);
            //AssertSuccessfulResult(response);
            //Assert.That(response.Result.SessionId);
            //Assert.That(response.Result.Username);
        }

        [Ignore("Ingoring...")]
        public void Upload_Throttling()
        {
            // Arrange
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\cat.jpg");
            var file = File.ReadAllBytes(path);
            var request = new UploadImageRequest(_sessionId, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");

            // Act
            var response = _api.Upload(request).Result;
            var response2 = _api.Upload(request).Result;
            var response3 = _api.Upload(request).Result;

            // Assert
            AssertFailedResult(response3);
            Assert.That(response3.Errors.Contains("Creating post is impossible. Please try 10 minutes later."));
        }

        private void AssertSuccessfulResult<T>(OperationResult<T> response)
        {
            lock (response)
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.True);
                Assert.That(response.Result, Is.Not.Null);
                Assert.That(response.Errors, Is.Empty);
            }
        }

        private void AssertFailedResult<T>(OperationResult<T> response)
        {
            lock (response)
            {
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Success, Is.False);
                Assert.That(response.Result, Is.Null);
                Assert.That(response.Errors, Is.Not.Empty);
            }
        }
    }
}