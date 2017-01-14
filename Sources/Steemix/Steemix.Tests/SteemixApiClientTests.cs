using System.IO;
using NUnit.Framework;
using Steemix.Library.Exceptions;
using Steemix.Library.HttpClient;
using Steemix.Library.Models.Requests;

namespace Steemix.Tests
{
    [TestFixture]
    public class SteemixApiClientTests
    {
        string _name = "joseph.kalu";
        string _password = "test1234";
        readonly SteemixApiClient _api = new SteemixApiClient();
        string _token = string.Empty;

        [OneTimeSetUp]
        public void Setup()
        {
            var request = new LoginRequest(_name, _password);
            _token = _api.Login(request).Token;
        }

        [Test]
        public void GetTopPostsTest()
        {
            // Arrange
            var request = new TopPostRequest(string.Empty, 10);

            // Act
            var response = _api.GetTopPosts(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsTrue(response.Results.Count > 0);
        }

        [Test]
        public void GetUserPostsTest()
        {
            // Arrange
            var request = new UserPostRequest(_token, _name);

            // Act
            var response = _api.GetUserPosts(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsTrue(response.Results.Count > 0);
        }

        [Test]
        public void GetUserAvatarTest()
        {
            // Arrange
            var request = new UserInfoRequest(_token, _name);

            // Act
            var response = _api.GetUserInfo(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsTrue(string.IsNullOrEmpty(response.error));
        }

        [Test]
        public void LoginTest()
        {
            // Arrange
            var request = new LoginRequest(_name, _password);

            // Act
            var response = _api.Login(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Token);
        }


        [Test]
        public void UploadImageTest()
        {
            // Arrange
            var file = File.ReadAllBytes(@"/home/anch/Pictures/cats.jpg");
            var request = new UploadImageRequest(_token, "Cats", file);

            // Act
            var response = _api.Upload(request);

            // Assert
            Assert.NotNull(response);
        }

        [Test]
        public void UpVoteTest_PostArchived()
        {
            // Arrange
            var request = new VoteRequest(_token, "@shenanigator/if-you-want-jobs-take-away-their-shovels-and-give-them-spoons");

            // Act
            var response = _api.UpVote(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsFalse(response.IsVoted);
        }

        [Test]
        public void DownVoteTest_PostArchived()
        {
            // Arrange
            var request = new VoteRequest(_token, "@shenanigator/if-you-want-jobs-take-away-their-shovels-and-give-them-spoons");

            // Act
            var response = _api.DownVote(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsFalse(response.IsVoted);
        }

        [Test]
        public void RegisterTest()
        {
            // Arrange
            var request = new RegisterRequest("5JdHigxo9s8rdNSfGteprcx1Fhi7SBUwb7e2UcNvnTdz18Si7so", "anch", "qwerty12345");

            // Act
            try
            {
                var response = _api.Register(request);

                // Assert
                Assert.NotNull(response);
                Assert.IsNotEmpty(response.username);
            }
            catch (ApiGatewayException ex)
            {
                Assert.True(ex.ResponseContent.Contains("A user with that username already exists"));
            }
        }

        [Test]
        public void GetCommentsTest()
        {
            // Arrange
            var request = new GetCommentsRequest(_token, "@asduj/new-application-coming---");

            // Act
            var response = _api.GetComments(request);

            // Assert
            Assert.NotNull(response);
            Assert.IsTrue(response.comments.Length > 0);
        }

        [Test]
        public void ChangePasswordTest()
        {
            // Arrange
            var request = new ChangePasswordRequest(_token, _password, _password);
            // Act
            var response = _api.ChangePassword(request);
            // Assert
            Assert.NotNull(response);
        }

        [Test]
        public void CreateCommentTest()
        {
            // Arrange
            var request = new CreateCommentsRequest(_token, "@asduj/new-application-coming---", "люк я твой отец", "лошта?");

            // Act
            var response = _api.CreateComment(request);

            // Assert
            Assert.NotNull(response);
        }

        [Test]
        public void FollowTest()
        {
            // Arrange
            var request = new FollowRequest(_token, "asduj");

            // Act
            var response = _api.Follow(request);

            // Assert
            Assert.NotNull(response);
        }

        [Test]
        public void UnfollowTest()
        {
            // Arrange
            var request = new FollowRequest(_token, "asduj");

            // Act
            var response = _api.Unfollow(request);

            // Assert
            Assert.NotNull(response);
        }
    }
}