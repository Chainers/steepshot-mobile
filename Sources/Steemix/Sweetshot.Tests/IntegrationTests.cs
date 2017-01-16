using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Sweetshot.Library.HttpClient;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;

namespace Sweetshot.Tests
{
    // check all tests
    // add more tests
    // test (assert) errors
    // remove throws in DTOs
    // check trello
    // check chat
    [TestFixture]
    public class IntegrationTests
    {
        private const string Name = "joseph.kalu";
        private const string Password = "test1234";
        private const string NewPassword = "test12345";
        private string _sessionId = string.Empty;

        private readonly SteepshotApiClient _api = new SteepshotApiClient(ConfigurationManager.AppSettings["sweetshot_url"]);

        [SetUp]
        public void Authenticate()
        {
            var request = new LoginRequest(Name, Password);
            _sessionId = _api.Login(request).Result.Result.SessionId;
        }

        [Test]
        public void Login_Valid_Credentials()
        {
            // Arrange
            var request = new LoginRequest(Name, Password);

            // Act
            var response = _api.Login(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That("User was logged in.", Is.EqualTo(response.Result.Message));
            Assert.That(response.Result.SessionId, Is.Not.Empty);
        }

        [Test]
        public void Login_Invalid_Credentials()
        {
            // Arrange
            var request = new LoginRequest(Name + "x", Password + "x");

            // Act
            var response = _api.Login(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Unable to login with provided credentials."));
        }

        [Test]
        public void Login_Wrong_Password()
        {
            // Arrange
            var request = new LoginRequest(Name, Password + "x");

            // Act
            var response = _api.Login(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Unable to login with provided credentials."));
        }

        [Test]
        public void Login_Wrong_Username()
        {
            // Arrange
            var request = new LoginRequest(Name + "x", Password);

            // Act
            var response = _api.Login(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Unable to login with provided credentials."));
        }

        [Test]
        public void UserPosts()
        {
            // Arrange
            var request = new UserPostsRequest(_sessionId, Name);

            // Act
            var response = _api.GetUserPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Title, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Category, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Empty);
            Assert.That(response.Result.Results.First().AuthorRewards, Is.Not.Null);
            Assert.That(response.Result.Results.First().AuthorReputation, Is.Not.Null);
            Assert.That(response.Result.Results.First().NetVotes, Is.Not.Null);
            Assert.That(response.Result.Results.First().Children, Is.Not.Null);
            Assert.That(response.Result.Results.First().Created, Is.Not.Null);
            Assert.That(response.Result.Results.First().CuratorPayoutValue, Is.Not.Null);
            Assert.That(response.Result.Results.First().TotalPayoutValue, Is.Not.Null);
            Assert.That(response.Result.Results.First().PendingPayoutValue, Is.Not.Null);
            Assert.That(response.Result.Results.First().MaxAcceptedPayout, Is.Not.Null);
            Assert.That(response.Result.Results.First().TotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Results.First().Vote, Is.False);
            Assert.That(response.Result.Results.First().Tags, Is.Empty);
            Assert.That(response.Result.Results.First().Depth, Is.Not.Zero);
        }

        [Test]
        public void UserPosts_Invalid_Username()
        {
            // Arrange
            var request = new UserPostsRequest(_sessionId, Name + "x");

            // Act
            var response = _api.GetUserPosts(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Cannot get posts for this username"));
        }

        [Test]
        public void UserRecentPosts()
        {
            // Arrange
            var request = new UserRecentPostsRequest(_sessionId);

            // Act
            var response = _api.GetUserRecentPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
        }

        [Test]
        public void Posts_Top()
        {
            // Arrange
            var request = new PostsRequest(PostType.Top);

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Posts_Top_Limit_Default()
        {
            // Arrange
            const int defaultLimit = 10;
            var request = new PostsRequest(PostType.Top);

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(defaultLimit, Is.EqualTo(response.Result.Count));
        }

        [Test]
        public void Posts_Top_Check_Limit()
        {
            // Arrange
            const int limit = 5;
            var request = new PostsRequest(PostType.Top, limit);

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(limit, Is.EqualTo(response.Result.Count));
        }

        [Test]
        public void Posts_Top_Check_Limit_Negative()
        {
            // Arrange
            const int defaultLimit = 10;
            var request = new PostsRequest(PostType.Top, -10);

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(defaultLimit, Is.EqualTo(response.Result.Count));
        }

        [Test]
        public void Posts_Top_Check_Offset()
        {
            // Arrange
            var request = new PostsRequest(PostType.Top, 3, "/life/@hanshotfirst/best-buddies-i-see-you");

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Offset, Is.Empty);
            Assert.That(response.Result.Count, Is.EqualTo(0));
            Assert.That(response.Result.Results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Posts_Hot()
        {
            // Arrange
            var request = new PostsRequest(PostType.Hot);

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Posts_New()
        {
            // Arrange
            var request = new PostsRequest(PostType.New);

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        // TODO Need to create a profile and test it
        [Ignore("Ignoring")]
        public void Register()
        {
            // Arrange
            var request = new RegisterRequest("", "", "");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            //AssertSuccessfulResult(response);
            //Assert.That(response.Result.SessionId);
            //Assert.That(response.Result.Username);
        }

        [Test]
        public void Register_PostingKey_Invalid()
        {
            // Arrange
            var request = new RegisterRequest("5JdHigxo9s8rdNSfGteprcx1Fhi7SBUwb7e2UcNvnTdz18Si7s1", "anch1", "qwerty12345");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Invalid posting key."));
        }

        [Test]
        public void Register_Username_Already_Exists()
        {
            // Arrange
            var request = new RegisterRequest("5JdHigxo9s8rdNSfGteprcx1Fhi7SBUwb7e2UcNvnTdz18Si7s1", "anch", "qwerty12345");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("A user with that username already exists."));
        }

        [Test]
        public void Register_PostingKey_Same_With_New_Username()
        {
            // Arrange
            var request = new RegisterRequest("5JdHigxo9s8rdNSfGteprcx1Fhi7SBUwb7e2UcNvnTdz18Si7s1", "anch1", "qwerty12345");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Invalid posting key."));
        }

        [Test]
        public void Register_PostingKey_Is_Blank()
        {
            // Arrange
            var request = new RegisterRequest("", "qweqweqweqwe", "qweqweqweqwe");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test]
        public void Register_Password_Is_Short()
        {
            // Arrange
            var request = new RegisterRequest("5JdHsgxo9s8rdNsfGteprcxaFhi7SBUwb7e2UcNvnTdh18Si7so", "qweqweqweqwe", "qweqweq");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This password is too short. It must contain at least 8 characters."));
        }

        [Test]
        public void Register_Password_Is_Numeric()
        {
            // Arrange
            var request = new RegisterRequest("5JdHsgxo9s8rdNsfGteprcxaFhi7SBUwb7e2UcNvnTdh18Si7so", "qweqweqweqwe", "1234567890");

            // Act
            var response = _api.Register(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This password is entirely numeric."));
        }

        [Test]
        public void Vote_Up()
        {
            // Prepare
            // TODO Create comment and vote


            // Arrange
            var request = new VoteRequest(_sessionId, true, "/nature/@joseph.kalu/test-post-abc2");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Message, Is.EqualTo("Upvoted"));
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
        }

        [Test]
        public void Vote_Down()
        {
            // Prepare
            // TODO Create comment and vote

            // Arrange
            var request = new VoteRequest(_sessionId, false, "/nature/@joseph.kalu/test-post-abc2");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Message, Is.EqualTo("Downvoted"));
            Assert.That(response.Result.NewTotalPayoutReward, Is.Not.Null);
        }

        [Test]
        public void Vote_Up_Already_Voted()
        {
            // Arrange
            var request = new VoteRequest(_sessionId, true, "/spam/@joseph.kalu/test-post-tue-jan--3-170111-2017");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("You have used the maximum number of vote changes on this comment."));
        }

        [Test]
        public void Vote_Down_Already_Voted()
        {
            // Arrange
            var request = new VoteRequest(_sessionId, false, "/spam/@joseph.kalu/test-post-tue-jan--3-170111-2017");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("You have used the maximum number of vote changes on this comment."));
        }

        [Test]
        public void Vote_Invalid_Identifier1()
        {
            // Arrange
            var request = new VoteRequest(_sessionId, true, "qwe");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test]
        public void Vote_Invalid_Identifier2()
        {
            // Arrange
            var request = new VoteRequest(_sessionId, true, "qwe/qwe");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test]
        public void Vote_Invalid_Identifier3()
        {
            // Arrange
            var request = new VoteRequest(_sessionId, true, "/qwe/qwe");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test]
        public void Vote_Invalid_Identifier4()
        {
            // Arrange
            var request = new VoteRequest(_sessionId, true, "/qwe/@qwe");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test]
        public void Vote_Invalid_Identifier5()
        {
            // Arrange
            var request = new VoteRequest(_sessionId, true, "/qwe/@qwe/");

            // Act
            var response = _api.Vote(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("You have used the maximum number of vote changes on this comment."));
        }

        [Test]
        public void Follow()
        {
            // Arrange
            var request = new FollowRequest(_sessionId, FollowType.Follow, "asduj");

            // Act
            var response = _api.Follow(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Message, Is.EqualTo("User is followed"));
        }

        [Test]
        public void Follow_UnFollow()
        {
            // Arrange
            var request = new FollowRequest(_sessionId, FollowType.UnFollow, "asduj");

            // Act
            var response = _api.Follow(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Message, Is.EqualTo("User is unfollowed"));
        }

        [Test]
        public void Follow_Invalid_Username()
        {
            // Arrange
            var request = new FollowRequest(_sessionId, FollowType.Follow, "qwet32qwe3qwewfoc020mm2nndasdwe");

            // Act
            var response = _api.Follow(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("User does not exist."));
        }

        [Test]
        public void Comments()
        {
            // Arrange
            var request = new GetCommentsRequest(_sessionId, "@asduj/new-application-coming---");

            // Act
            var response = _api.GetComments(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Title, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Category, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Empty);
            Assert.That(response.Result.Results.First().AuthorRewards, Is.Not.Null);
            Assert.That(response.Result.Results.First().AuthorReputation, Is.Not.Null);
            Assert.That(response.Result.Results.First().NetVotes, Is.Not.Null);
            Assert.That(response.Result.Results.First().Children, Is.Not.Null);
            Assert.That(response.Result.Results.First().Created, Is.Not.Null);
            Assert.That(response.Result.Results.First().CuratorPayoutValue, Is.Not.Null);
            Assert.That(response.Result.Results.First().TotalPayoutValue, Is.Not.Null);
            Assert.That(response.Result.Results.First().PendingPayoutValue, Is.Not.Null);
            Assert.That(response.Result.Results.First().MaxAcceptedPayout, Is.Not.Null);
            Assert.That(response.Result.Results.First().TotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Results.First().Vote, Is.False);
            Assert.That(response.Result.Results.First().Tags, Is.Empty);
            Assert.That(response.Result.Results.First().Depth, Is.Not.Zero);
        }

        [Test]
        public void Comments_Invalid_Url()
        {
            // Arrange
            var request = new GetCommentsRequest(_sessionId, "qwe");

            // Act
            var response = _api.GetComments(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
        }

        [Test]
        public void Comments_Invalid_Url_But_Valid_User()
        {
            // Arrange
            var request = new GetCommentsRequest(_sessionId, "@asduj/qweqweqweqw");

            // Act
            var response = _api.GetComments(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
        }

        [Test]
        public void CreateComment()
        {
            // Arrange
            var request = new CreateCommentsRequest(_sessionId, "/spam/@joseph.kalu/test-post-127", "хипстеры наелись фалафели в коворкинге", "свитшот");

            // Act
            var response = _api.CreateComment(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Message, Is.EqualTo("Comment created"));
        }

        [Test]
        public void CreateComment_Wrong_Identifier()
        {
            // Arrange
            var request = new CreateCommentsRequest(_sessionId, "@asduj/new-application-coming---", "хипстеры наелись фалафели в коворкинге", "свитшот");

            // Act
            var response = _api.CreateComment(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
        }

        [Test]
        public void CreateComment_Empty_Body()
        {
            // Arrange
            var request = new CreateCommentsRequest(_sessionId, "/spam/@joseph.kalu/test-post-127", "", "свитшот");

            // Act
            var response = _api.CreateComment(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test]
        public void CreateComment_Empty_Title()
        {
            // Arrange
            var request = new CreateCommentsRequest(_sessionId, "/spam/@joseph.kalu/test-post-127", "свитшот", "");

            // Act
            var response = _api.CreateComment(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test]
        public void Upload()
        {
            // Arrange
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\cat.jpg");
            var file = File.ReadAllBytes(path);
            var request = new UploadImageRequest(_sessionId, "cat", file, "cat1", "cat2", "cat3", "cat4");

            // Act
            var response = _api.Upload(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Body, Is.Not.Empty);
            Assert.That(response.Result.Title, Is.Not.Empty);
            Assert.That(response.Result.Tags, Is.Not.Empty);
        }

        [Test]
        public void Categories()
        {
            // Arrange
            var request = new CategoriesRequest(_sessionId);

            // Act
            var response = _api.GetCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        public void Categories_Offset()
        {
            // Arrange
            var request = new CategoriesRequest(_sessionId, "food");

            // Act
            var response = _api.GetCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("food"));
        }

        [Test]
        public void Categories_Offset_Empty()
        {
            // Arrange
            var request = new CategoriesRequest(_sessionId, " ");

            // Act
            var response = _api.GetCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Categories_Offset_Not_Exisiting()
        {
            // Arrange
            var request = new CategoriesRequest(_sessionId, "qweqweqwe");

            // Act
            var response = _api.GetCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Categories_Limit()
        {
            // Arrange
            const int limit = 5;
            var request = new CategoriesRequest(_sessionId, limit: limit);

            // Act
            var response = _api.GetCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
        }

        [Test]
        public void Categories_Limit_Negative()
        {
            // Arrange
            const int limit = -5;
            const int defaultLimit = 10;
            var request = new CategoriesRequest(_sessionId, limit: limit);

            // Act
            var response = _api.GetCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Count, Is.EqualTo(defaultLimit));
        }

        [Test]
        public void Categories_Search()
        {
            // Arrange
            var request = new SearchCategoriesRequest(_sessionId, "foo");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount >= 0);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        public void Categories_Search_Invalid_Query()
        {
            // Arrange
            var request = new SearchCategoriesRequest(_sessionId, "qwerqwerqwerqwerqwerqwerqwerqwer");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Empty);
            Assert.That(response.Result.Count, Is.EqualTo(0));
            Assert.That(response.Result.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public void Categories_Search_Short_Query()
        {
            // Arrange
            var request = new SearchCategoriesRequest(_sessionId, "fo");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert 
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Query should have at least 3 characters"));
        }

        [Test]
        public void Categories_Search_Empty_Query()
        {
            // Arrange
            var request = new SearchCategoriesRequest(_sessionId, "");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert 
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test]
        public void ChangePassword()
        {
            // Arrange
            var request = new ChangePasswordRequest(_sessionId, Password, NewPassword);

            // Act
            var response = _api.ChangePassword(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Message, Is.EqualTo("Password was changed"));

            // Revert
            var loginResponse = _api.Login(new LoginRequest(Name, NewPassword)).Result;
            var response2 = _api.ChangePassword(new ChangePasswordRequest(loginResponse.Result.SessionId, NewPassword, Password)).Result;
            AssertSuccessfulResult(response2);
            Assert.That(response2.Result.Message, Is.EqualTo("Password was changed"));
        }

        [Test]
        public void ChangePassword_Invalid_OldPassword()
        {
            // Arrange
            var request = new ChangePasswordRequest(_sessionId, Password + "x", NewPassword);

            // Act
            var response = _api.ChangePassword(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Old password is invalid."));
        }

        // TODO Add more tests about password types
        [Test]
        public void ChangePassword_NewPassword_Short()
        {
            // Arrange
            var request = new ChangePasswordRequest(_sessionId, Password, "t");

            // Act
            var response = _api.ChangePassword(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This password is too short. It must contain at least 8 characters."));
        }

        [Test]
        public void ChangePassword_Invalid_SessionId()
        {
            // Arrange
            var request = new ChangePasswordRequest(_sessionId + "x", Password, NewPassword);

            // Act
            var response = _api.ChangePassword(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Authentication credentials were not provided."));
        }

        [Test]
        public void Logout()
        {
            // Arrange
            var request = new LogoutRequest(_sessionId);

            // Act
            var response = _api.Logout(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Message, Is.EqualTo("User is logged out"));
        }

        [Test]
        public void UserProfile()
        {
            // Arrange
            var request = new UserProfileRequest(_sessionId, Name);

            // Act
            var response = _api.GetUserProfile(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.PostingRewards, Is.Not.Null);
            Assert.That(response.Result.CurationRewards, Is.Not.Null);
            Assert.That(response.Result.LastAccountUpdate, Is.Not.Null);
            Assert.That(response.Result.LastVoteTime, Is.Not.Null);
            Assert.That(response.Result.Reputation, Is.Not.Null);
            Assert.That(response.Result.PostCount, Is.Not.Null);
            Assert.That(response.Result.CommentCount, Is.Not.Null);
            Assert.That(response.Result.FollowersCount, Is.Not.Null);
            Assert.That(response.Result.FollowingCount, Is.Not.Null);
            Assert.That(response.Result.Username, Is.Not.Empty);
            Assert.That(response.Result.CurrentUsername, Is.Not.Empty);
            Assert.That(response.Result.ProfileImage, Is.Not.Empty);
            Assert.That(response.Result.HasFollowed, Is.Not.Null);
            Assert.That(response.Result.EstimatedBalance, Is.Not.Null);
        }

        [Test]
        public void UserProfile_Invalid_Username()
        {
            // Arrange
            var request = new UserProfileRequest(_sessionId, "qweqweqwe");

            // Act
            var response = _api.GetUserProfile(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("User not found"));
        }

        [Test]
        public void UserFriends_Following()
        {
            // Arrange
            var request = new UserFriendsRequest(_sessionId, Name, FriendsType.Following);

            // Act
            var response = _api.GetUserFriends(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Reputation, Is.Not.Null);
        }

        [Test]
        public void UserFriends_Followers()
        {
            // Arrange
            var request = new UserFriendsRequest(_sessionId, Name, FriendsType.Followers);

            // Act
            var response = _api.GetUserFriends(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
        }

        [Test]
        public void UserFriends_Followers_Invalid_Username()
        {
            // Arrange
            var request = new UserFriendsRequest(_sessionId, Name + "x", FriendsType.Followers);

            // Act
            var response = _api.GetUserFriends(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count == 0);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        public void UserFriends_Followers_Offset()
        {
            // Arrange
            var request = new UserFriendsRequest(_sessionId, Name, FriendsType.Followers, "vivianupman");

            // Act
            var response = _api.GetUserFriends(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.EqualTo("vivianupman"));
        }

        [Test]
        public void UserFriends_Followers_Limit()
        {
            // Arrange
            var request = new UserFriendsRequest(_sessionId, Name, FriendsType.Followers, limit: 5);

            // Act
            var response = _api.GetUserFriends(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Count == 5);
        }

        [Test]
        public void UserFriends_Followers_Limit_Negative()
        {
            // Arrange
            const int defaultLimit = 50;
            var request = new UserFriendsRequest(_sessionId, Name, FriendsType.Followers, limit: -5);

            // Act
            var response = _api.GetUserFriends(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Count, Is.EqualTo(defaultLimit));
        }

        [Test]
        public void Terms_Of_Service()
        {
            // Arrange
            // Act
            var response = _api.TermsOfService().Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Text, Is.Not.Empty);
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