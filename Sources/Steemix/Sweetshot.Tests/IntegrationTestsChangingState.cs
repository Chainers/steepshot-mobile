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
        [Order(0)]
        public void Upload()
        {
            // Arrange
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\cat.jpg");
            var file = File.ReadAllBytes(path);
            var request = new UploadImageRequest(_sessionId, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");

            // Act
            var response = _api.Upload(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Body, Is.Not.Empty);
            Assert.That(response.Result.Title, Is.Not.Empty);
            Assert.That(response.Result.Tags, Is.Not.Empty);
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

        [Test]
        [Order(1)]
        public void CreateComment()
        {
            var userPostsRequest = new UserPostsRequest(Name);
            var lastPost = _api.GetUserPosts(userPostsRequest).Result.Result.Results.First();

            // Arrange
            var request = new CreateCommentRequest(_sessionId, lastPost.Url, "nailed it !", "свитшот");

            // Act
            var response = _api.CreateComment(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsCreated, Is.True);
            Assert.That(response.Result.Message, Is.EqualTo("Comment created"));
        }

        [Test]
        [Order(2)]
        public void Vote_Up()
        {
            // Prepare
            var userPostsRequest = new UserPostsRequest(Name);
            var userPostsResponse = _api.GetUserPosts(userPostsRequest).Result;
            AssertSuccessfulResult(userPostsResponse);
            Assert.That(userPostsResponse.Result.Count, Is.Not.Null);
            Assert.That(userPostsResponse.Result.Offset, Is.Not.Empty);
            Assert.That(userPostsResponse.Result.Results, Is.Not.Empty);

            // Arrange
            var url = userPostsResponse.Result.Results.First().Url;
            var request = new VoteRequest(_sessionId, true, url);

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsVoted, Is.True);
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
        }

        [Test]
        [Order(3)]
        public void Vote_Down()
        {
            // Prepare
            var userPostsRequest = new UserPostsRequest(Name);
            var userPostsResponse = _api.GetUserPosts(userPostsRequest).Result;
            AssertSuccessfulResult(userPostsResponse);
            Assert.That(userPostsResponse.Result.Count, Is.Not.Null);
            Assert.That(userPostsResponse.Result.Offset, Is.Not.Empty);
            Assert.That(userPostsResponse.Result.Results, Is.Not.Empty);

            // Arrange
            var url = userPostsResponse.Result.Results.First().Url;

            // Arrange
            var request = new VoteRequest(_sessionId, false, url);

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsVoted, Is.False);
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Message, Is.EqualTo("Downvoted"));
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
        }

        [Test]
        public void Vote_Up_Comment()
        {
            // Get latest posts
            var userPostsRequest = new UserPostsRequest(Name);
            var userPostsResponse = _api.GetUserPosts(userPostsRequest).Result;
            AssertSuccessfulResult(userPostsResponse);
            Assert.That(userPostsResponse.Result.Count, Is.Not.Null);
            Assert.That(userPostsResponse.Result.Offset, Is.Not.Empty);
            Assert.That(userPostsResponse.Result.Results, Is.Not.Empty);

            var postUrl = userPostsResponse.Result.Results.First().Url;

            // Comment latest post
            var createCommentRequest = new CreateCommentRequest(_sessionId, postUrl, "nailed it !", "свитшот");
            var createCommentResponse = _api.CreateComment(createCommentRequest).Result;
            AssertSuccessfulResult(createCommentResponse);
            Assert.That(createCommentResponse.Result.IsCreated, Is.True);
            Assert.That(createCommentResponse.Result.Message, Is.EqualTo("Comment created"));

            // Load comments for this post
            var commentsRequest = new GetCommentsRequest(postUrl);
            var commentsResponse = _api.GetComments(commentsRequest).Result;
            AssertSuccessfulResult(commentsResponse);
            Assert.That(commentsResponse.Result.Count, Is.Not.Null);
            Assert.That(commentsResponse.Result.Results, Is.Not.Empty);

            // Voteup new comment
            var commentUrl = commentsResponse.Result.Results.First().Url.Split('#').Last();
            var request = new VoteRequest(_sessionId, true, commentUrl);

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsVoted, Is.True);
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);

            // Check if it is was voted
            Thread.Sleep(TimeSpan.FromSeconds(5));
            var commentsRequest2 = new GetCommentsRequest(postUrl) {SessionId = _sessionId};
            var commentsResponse2 = _api.GetComments(commentsRequest2).Result;
            Assert.That(commentsResponse2.Result.Results.First().Vote, Is.True);
        }

        [Test]
        public void Vote_Down_Comment()
        {
            //TODO
        }

        [Test]
        public void Create_Post_Vote_Check()
        {
            // Create new post
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\cat.jpg");
            var file = File.ReadAllBytes(path);
            var createPostRequest = new UploadImageRequest(_sessionId, "cat" + DateTime.UtcNow.Ticks, file, "cat1", "cat2", "cat3", "cat4");
            var createPostResponse = _api.Upload(createPostRequest).Result;

            // Check if it is there
            var userPostsResponse = _api.GetUserPosts(new UserPostsRequest(Name)).Result;
            Assert.That(createPostResponse.Result.Title, Is.EqualTo(userPostsResponse.Result.Results.First().Title));
            Assert.That(userPostsResponse.Result.Results.First().Vote, Is.False);

            // Arrange
            var url = userPostsResponse.Result.Results.First().Url;
            var request = new VoteRequest(_sessionId, true, url);

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsVoted, Is.True);
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);

            // Check if it is was voted
            Thread.Sleep(TimeSpan.FromSeconds(5));
            var userPostsResponse2 = _api.GetUserPosts(new UserPostsRequest(Name) {SessionId = _sessionId}).Result;
            Assert.That(userPostsResponse2.Result.Results.First().Vote, Is.True);
        }

        [Test]
        public void Create_Comment_Vote_Check()
        {
            //TODO
        }

        [Test]
        [Order(4)]
        public void Follow()
        {
            // Arrange
            var request = new FollowRequest(_sessionId, FollowType.Follow, "asduj");

            // Act
            var response = _api.Follow(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsFollowed, Is.True);
            Assert.That(response.Result.Message, Is.EqualTo("User is followed"));
        }

        [Test]
        [Order(5)]
        public void Follow_UnFollow()
        {
            // Arrange
            var request = new FollowRequest(_sessionId, FollowType.UnFollow, "asduj");

            // Act
            var response = _api.Follow(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsFollowed, Is.False);
            Assert.That(response.Result.Message, Is.EqualTo("User is unfollowed"));
        }

        [Test]
        [Order(6)]
        public void Logout()
        {
            // Arrange
            var request = new LogoutRequest(_sessionId);

            // Act
            var response = _api.Logout(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsLoggedOut, Is.True);
            Assert.That(response.Result.Message, Is.EqualTo("User is logged out"));
        }

        [Ignore("Ignoring")]
        [Order(7)]
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

        [Test]
        [Order(8)]
        public void ChangePassword()
        {
            // Arrange
            var request = new ChangePasswordRequest(_sessionId, Password, NewPassword);

            // Act
            var response = _api.ChangePassword(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsChanged, Is.True);
            Assert.That(response.Result.Message, Is.EqualTo("Password was changed"));

            // Revert
            var loginResponse = _api.Login(new LoginRequest(Name, NewPassword)).Result;
            var response2 = _api.ChangePassword(new ChangePasswordRequest(loginResponse.Result.SessionId, NewPassword, Password)).Result;
            AssertSuccessfulResult(response2);
            Assert.That(response.Result.IsChanged, Is.True);
            Assert.That(response2.Result.Message, Is.EqualTo("Password was changed"));
            Authenticate();
        }

        private void AssertSuccessfulResult<T>(OperationResult<T> response)
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.True);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Errors, Is.Empty);
        }

        private void AssertFailedResult<T>(OperationResult<T> response)
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Success, Is.False);
            Assert.That(response.Result, Is.Null);
            Assert.That(response.Errors, Is.Not.Empty);
        }
    }
}