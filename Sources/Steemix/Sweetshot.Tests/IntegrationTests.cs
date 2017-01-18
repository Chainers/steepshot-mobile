using System.Configuration;
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
    // {"detail":["Creating post is impossible. Please try 10 minutes later."]}

    [TestFixture]
    public class IntegrationTests
    {
        private const string Name = "joseph.kalu";
        private const string Password = "test1234";
        private const string NewPassword = "test12345";
        private string _sessionId = string.Empty;

        private readonly SteepshotApiClient _api = new SteepshotApiClient(ConfigurationManager.AppSettings["sweetshot_url"]);

        [OneTimeSetUp]
        public void Authenticate()
        {
            // Arrange
            var request = new LoginRequest(Name, Password);

            // Act
            var response = _api.Login(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.IsLoggedIn, Is.True);
            Assert.That("User was logged in.", Is.EqualTo(response.Result.Message));
            Assert.That(response.Result.SessionId, Is.Not.Empty);

            // Setup
            _sessionId = _api.Login(request).Result.Result.SessionId;
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
            var request = new UserPostsRequest(Name);

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
            Assert.That(response.Result.Results.First().Tags, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Depth, Is.Not.Null);
        }

        [Test]
        public void UserPosts_Invalid_Username()
        {
            // Arrange
            var request = new UserPostsRequest(Name + "x");

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
            var request = new PostsRequest(PostType.Top, limit: limit);

            // Act
            var response = _api.GetPosts(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(limit, Is.EqualTo(response.Result.Count));
        }

        [Test]
        public void Posts_Top_Check_Offset()
        {
            // Arrange
            var request = new PostsRequest(PostType.Top, "/life/@hanshotfirst/best-buddies-i-see-you", 3);

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
            var request = new GetCommentsRequest("@joseph.kalu/cat636203355240074655");

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
            var request = new GetCommentsRequest("qwe");

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
            var request = new GetCommentsRequest("@asduj/qweqweqweqw");

            // Act
            var response = _api.GetComments(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
        }

        [Test]
        public void CreateComment_Wrong_Identifier()
        {
            // Arrange
            var request = new CreateCommentRequest(_sessionId, "@asduj/new-application-coming---", "test_body", "test_title");

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
            var request = new CreateCommentRequest(_sessionId, "/spam/@joseph.kalu/test-post-127", "", "test_title");

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
            var request = new CreateCommentRequest(_sessionId, "/spam/@joseph.kalu/test-post-127", "test_body", "");

            // Act
            var response = _api.CreateComment(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test]
        public void Categories()
        {
            // Arrange
            var request = new CategoriesRequest();

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
            var request = new CategoriesRequest("food");

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
        public void Categories_Offset_Not_Exisiting()
        {
            // Arrange
            var request = new CategoriesRequest("qweqweqwe");

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
            var request = new CategoriesRequest(limit: limit);

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
        public void Categories_Search()
        {
            // Arrange
            var request = new SearchCategoriesRequest("foo");

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
            var request = new SearchCategoriesRequest("qwerqwerqwerqwerqwerqwerqwerqwer");

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
            var request = new SearchCategoriesRequest("fo");

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
            var request = new SearchCategoriesRequest(" ");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert 
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test]
        public void Categories_Search_Test_Limit()
        {
            // Arrange
            const int defaultLimit = 50;
            var request = new SearchCategoriesRequest("steem");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count, Is.EqualTo(defaultLimit));
            Assert.That(response.Result.TotalCount >= defaultLimit);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        public void Categories_Search_Offset()
        {
            // Arrange
            var request = new SearchCategoriesRequest("life", "lifetime");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount > 0);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("lifetime"));
        }

        [Test]
        public void Categories_Search_Offset_Not_Exisiting()
        {
            // Arrange
            var request = new SearchCategoriesRequest("life", "qweqweqwe");

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Category used for offset was not found"));
        }

        [Test]
        public void Categories_Search_Limit()
        {
            // Arrange
            const int limit = 5;
            var request = new SearchCategoriesRequest("life", limit: limit);

            // Act
            var response = _api.SearchCategories(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Count, Is.EqualTo(limit));
            Assert.That(response.Result.TotalCount > 0);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
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
        public void ChangePassword_NewPassword_Numeric()
        {
            // Arrange
            var request = new ChangePasswordRequest(_sessionId, Password, "1234567890");

            // Act
            var response = _api.ChangePassword(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("This password is entirely numeric."));
        }

        [Test]
        public void UserProfile()
        {
            // Arrange
            var request = new UserProfileRequest(Name);

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
            Assert.That(response.Result.CurrentUsername, Is.Not.Null);
            Assert.That(response.Result.ProfileImage, Is.Not.Empty);
            Assert.That(response.Result.HasFollowed, Is.Not.Null);
            Assert.That(response.Result.EstimatedBalance, Is.Not.Null);
        }

        [Test]
        public void UserProfile_Invalid_Username()
        {
            // Arrange
            var request = new UserProfileRequest("qweqweqwe");

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
            var request = new UserFriendsRequest(Name, FriendsType.Following);

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
            var request = new UserFriendsRequest(Name, FriendsType.Followers);

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
            var request = new UserFriendsRequest(Name + "x", FriendsType.Followers);

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
            var request = new UserFriendsRequest(Name, FriendsType.Followers, "vivianupman");

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
            var request = new UserFriendsRequest(Name, FriendsType.Followers, limit: 5);

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
        public void Terms_Of_Service()
        {
            // Arrange
            // Act
            var response = _api.TermsOfService().Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Text, Is.Not.Empty);
        }

        [Test]
        public void GetPostInfo()
        {
            // Arrange
            var request = new PostsInfoRequest("/spam/@joseph.kalu/test-post-127");

            // Act
            var response = _api.GetPostInfo(request).Result;

            // Assert
            AssertSuccessfulResult(response);
            Assert.That(response.Result.Body, Is.Not.Empty);
            Assert.That(response.Result.Title, Is.Not.Empty);
            Assert.That(response.Result.Url, Is.Not.Empty);
            Assert.That(response.Result.Category, Is.Not.Empty);
            Assert.That(response.Result.Author, Is.Not.Empty);
            Assert.That(response.Result.Avatar, Is.Not.Empty);
            Assert.That(response.Result.AuthorRewards, Is.Not.Null);
            Assert.That(response.Result.AuthorReputation, Is.Not.Null);
            Assert.That(response.Result.NetVotes, Is.Not.Null);
            Assert.That(response.Result.Children, Is.Not.Null);
            Assert.That(response.Result.Created, Is.Not.Null);
            Assert.That(response.Result.CuratorPayoutValue, Is.Not.Null);
            Assert.That(response.Result.TotalPayoutValue, Is.Not.Null);
            Assert.That(response.Result.PendingPayoutValue, Is.Not.Null);
            Assert.That(response.Result.MaxAcceptedPayout, Is.Not.Null);
            Assert.That(response.Result.TotalPayoutReward, Is.Not.Null);
            Assert.That(response.Result.Vote, Is.False);
            Assert.That(response.Result.Tags, Is.Not.Null);
            Assert.That(response.Result.Depth, Is.Not.Null);
        }

        [Test]
        public void GetPostInfo_Invalid_Url()
        {
            // Arrange
            var request = new PostsInfoRequest("/spam/@joseph.kalu/qweqeqwqweqweqwe");

            // Act
            var response = _api.GetPostInfo(request).Result;

            // Assert
            AssertFailedResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
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