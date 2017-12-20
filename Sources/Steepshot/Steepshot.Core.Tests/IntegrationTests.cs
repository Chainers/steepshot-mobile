using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class IntegrationTests : BaseTests
    {
        [Test]
        public void Login_With_Posting_Key_Invalid_Credentials([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            user.Login += "x";
            user.PostingKey += "x";
            var request = new AuthorizedRequest(user);

            // Act
            var response = Api[apiName].LoginWithPostingKey(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Invalid private posting key.")
                        || response.Error.Message.Contains("Invalid posting key.")
                        || response.Error.Message.Contains(Localization.Errors.WrongPrivateKey));
        }

        [Test]
        public void Login_With_Posting_Key_Wrong_PostingKey([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            user.PostingKey += "x";
            var request = new AuthorizedRequest(user);

            // Act
            var response = Api[apiName].LoginWithPostingKey(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Invalid private posting key.")
                        || response.Error.Message.Contains("Invalid posting key.")
                        || response.Error.Message.Contains(Localization.Errors.WrongPrivateKey));
        }

        [Test]
        public void Login_With_Posting_Key_Wrong_Username([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            user.Login += "x";
            var request = new AuthorizedRequest(user);

            // Act
            var response = Api[apiName].LoginWithPostingKey(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Invalid private posting key.")
                        || response.Error.Message.Contains("Invalid posting key.")
                        || response.Error.Message.Contains(Localization.Errors.WrongPrivateKey));
        }

        [Test]
        public void UserPosts([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserPostsRequest(user.Login);
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = Api[apiName].GetUserPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Title, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Category, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Null);
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
            Assert.That(response.Result.Results.First().Vote, Is.Not.Null);
            Assert.That(response.Result.Results.First().Tags, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Depth, Is.Not.Null);
        }

        [Test]
        public void UserPosts_Invalid_Username([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserPostsRequest(user.Login + "x");

            // Act
            var response = Api[apiName].GetUserPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Cannot get posts for this username"));
        }

        [Test, Sequential]
        public void UserPosts_Offset_Limit(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("/steepshot/@joseph.kalu/cat636416737569422613-2017-09-22-10-42-38", "/steepshot/@joseph.kalu/cat636416737747907631-2017-09-22-10-42-56")] string offset)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserPostsRequest(user.Login);
            request.Offset = offset;
            request.Limit = 3;
            request.ShowLowRated = true;
            request.ShowNsfw = true;

            // Act
            var response = Api[apiName].GetUserPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        public void UserPosts_With_User_Some_Votes_True([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserPostsRequest(user.Login) { Login = user.Login };
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = Api[apiName].GetUserPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.True);
        }

        [Test]
        public void UserPosts_Without_User_All_Votes_False([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserPostsRequest(user.Login);

            // Act
            var response = Api[apiName].GetUserPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.False);
        }

        [Test]
        public void UserRecentPosts([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new CensoredNamedRequestWithOffsetLimitFields
            {
                Login = user.Login
            };

            // Act
            var response = Api[apiName].GetUserRecentPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
        }

        [Test]
        public void UserRecentPosts_Offset_Limit([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new CensoredNamedRequestWithOffsetLimitFields
            {
                Login = user.Login
            };
            request.Offset = Api[apiName].GetUserRecentPosts(request, CancellationToken.None).Result.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = Api[apiName].GetUserRecentPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        public void Posts_Top([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new PostsRequest(PostType.Top);

            // Act
            var response = Api[apiName].GetPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Posts_Top_Limit_Default([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            const int defaultLimit = 20;
            var request = new PostsRequest(PostType.Top);

            // Act
            var response = Api[apiName].GetPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
        }

        [Test]
        public void Posts_Hot_Offset_Limit([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new PostsRequest(PostType.Hot);
            request.Offset = Api[apiName].GetPosts(request, CancellationToken.None).Result.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = Api[apiName].GetPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        public void Posts_Top_With_User([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new PostsRequest(PostType.Top) { Login = user.Login };

            // Act
            var response = Api[apiName].GetPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
        }

        [Test]
        public void Posts_Hot([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new PostsRequest(PostType.Hot);

            // Act
            var response = Api[apiName].GetPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Posts_New([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new PostsRequest(PostType.New);

            // Act
            var response = Api[apiName].GetPosts(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Posts_By_Category([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, category);

            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test]
        public void Posts_By_Category_Invalid_Name([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, "asdas&^@dsad__sa@@d sd222f_f");

            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Not Found"));
        }

        [Test]
        public void Posts_By_Category_Not_Existing_Name([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, "qweqweqweqewqwqweqe");

            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        public void Posts_By_Category_Empty_Name([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, "");

            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Not Found"));
        }

        [Test]
        public void Posts_By_Category_Hot([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Hot, category);

            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test]
        public void Posts_By_Category_New([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryRequest(PostType.New, category);

            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test]
        public void Posts_By_Category_Offset_Limit([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, category);
            request.Offset = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result.Result.Results.First().Url;
            request.Limit = 5;

            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Where(x => !x.Tags.Contains(category)), Is.Empty);
            Assert.That(response.Result.Results.Count, Is.EqualTo(request.Limit));
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        public void Posts_By_Category_With_User([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            UserInfo user = Users[apiName];
            var request = new PostsByCategoryRequest(PostType.Top, category) { Login = user.Login };
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = Api[apiName].GetPostsByCategory(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        public void Vote_Up_Already_Voted([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Load last post
            UserInfo user = Users[apiName];
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            var lastPost = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Up, lastPost.Url);

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;
            Thread.Sleep(2000);
            var response2 = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You have already voted in a similar way.")
                        || response2.Error.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Error.Message.Contains("Cannot vote again on a comment after payout.")
                        || response2.Error.Message.Contains("Duplicate transaction check failed")
                        || response2.Error.Message.Contains("Vote weight cannot be 0.")
                        || response2.Error.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Error.Message);
        }

        [Test]
        public void Vote_Down_Already_Voted([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Load last post
            UserInfo user = Users[apiName];
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var lastPost = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Down, lastPost.Url);

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;
            Thread.Sleep(2000);
            var response2 = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You have already voted in a similar way.")
                        || response2.Error.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Error.Message.Contains("Duplicate transaction check failed")
                        || response2.Error.Message.Contains("Vote weight cannot be 0.")
                        || response2.Error.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Error.Message);
        }

        [Test]
        public void Vote_Invalid_Identifier1([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Up, "qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        public void Vote_Invalid_Identifier2([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Up, "qwe/qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        public void Vote_Invalid_Identifier3([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Up, "qwe/qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        public void Vote_Invalid_Identifier4([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Up, "qwe/@qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        public void Flag_Up_Already_Flagged([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Load last post
            UserInfo user = Users[apiName];
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var lastPost = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Flag, lastPost.Url);

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;
            var response2 = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You have already voted in a similar way.")
                        || response2.Error.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Error.Message.Contains("Duplicate transaction check failed")
                        || response2.Error.Message.Contains("Vote weight cannot be 0.")
                        || response2.Error.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Error.Message);
        }

        [Test]
        public void Flag_Down_Already_Flagged([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Load last post
            UserInfo user = Users[apiName];
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var lastPost = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Down, lastPost.Url);

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;
            var response2 = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response2);
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You have already voted in a similar way.")
                        || response2.Error.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Error.Message.Contains("Duplicate transaction check failed")
                        || response2.Error.Message.Contains("Vote weight cannot be 0.")
                        || response2.Error.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Error.Message);
        }

        [Test]
        public void Flag_Invalid_Identifier1([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Flag, "qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test]
        public void Flag_Invalid_Identifier2([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Flag, "qwe/qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test]
        public void Flag_Invalid_Identifier3([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Flag, "qwe/qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test]
        public void Flag_Invalid_Identifier4([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new VoteRequest(Users[apiName], VoteType.Flag, "qwe/@qwe");

            // Act
            var response = Api[apiName].Vote(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test, Sequential]
        public void Comments(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("@joseph.kalu/cat636203355240074655", "@joseph.kalu/cat636281384922864910")] string url)
        {
            // Arrange
            var request = new NamedInfoRequest(url);

            // Act
            var response = Api[apiName].GetComments(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Title, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Category, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Null);
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
            Assert.That(response.Result.Results.First().Vote, Is.Not.Null);
            Assert.That(response.Result.Results.First().Tags, Is.Empty);
            Assert.That(response.Result.Results.First().Depth, Is.Not.Zero);
        }

        [Test, Sequential]
        public void Comments_With_User_Check_True_Votes(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("@joseph.kalu/cat636203355240074655", "@joseph.kalu/egregious-2017-10-16-10-48-02")] string url)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new NamedInfoRequest(url) { Login = user.Login };

            // Act
            var response = Api[apiName].GetComments(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.True);
        }

        [Test, Sequential]
        public void Comments_Without_User_Check_False_Votes(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("@dollarvigilante/could-ethereum-be-made-obsolete-by-the-new-decentralized-smart-contract-platform-eos", "@siberianshamen/chto-takoe-golos")] string url)
        {
            // Arrange
            var request = new NamedInfoRequest(url);

            // Act
            var response = Api[apiName].GetComments(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.False);
        }

        [Test]
        public void Comments_Invalid_Url([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new NamedInfoRequest("qwe");

            // Act
            var response = Api[apiName].GetComments(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Wrong identifier."));
        }

        [Test]
        public void Comments_Invalid_Url_But_Valid_User([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new NamedInfoRequest("@asduj/qweqweqweqw");

            // Act
            var response = Api[apiName].GetComments(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Wrong identifier."));
        }

        [Test]
        public void CreateComment_20_Seconds_Delay([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            var userPostsResponse = Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None).Result;
            var lastPost = userPostsResponse.Result.Results.First();
            const string body = "Ллойс!";
            var createCommentRequest = new CommentRequest(Users[apiName], lastPost.Url, body, AppSettings.AppInfo);

            // Act
            var response1 = Api[apiName].CreateComment(createCommentRequest, CancellationToken.None).Result;
            var response2 = Api[apiName].CreateComment(createCommentRequest, CancellationToken.None).Result;

            // Assert
            AssertResult(response1);
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You may only comment once every 20 seconds.") || response2.Error.Message.Contains("Duplicate transaction check failed"), response2.Error.Message);
        }

        [Test]
        public void EditCommentTest([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var api = Api[apiName];
            var userPostsRequest = new UserPostsRequest(user.Login);
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            var userPostsResponse = api.GetUserPosts(userPostsRequest, CancellationToken.None).Result;

            var post = userPostsResponse.Result.Results.FirstOrDefault(i => i.Children > 0);
            Assert.IsNotNull(post);
            var namedRequest = new NamedInfoRequest(post.Url);
            var comments = api.GetComments(namedRequest, CancellationToken.None).Result;
            var comment = comments.Result.Results.FirstOrDefault(i => i.Author.Equals(user.Login));
            Assert.IsNotNull(comment);

            var editCommentRequest = new CommentRequest(user, comment.Url, comment.Body += $" edited {DateTime.Now}", AppSettings.AppInfo);

            var result = Api[apiName].EditComment(editCommentRequest, CancellationToken.None).Result;
            AssertResult(result);
        }

        [Test]
        public void Categories([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new OffsetLimitFields();

            // Act
            var response = Api[apiName].GetCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        public void Categories_Offset_Limit([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            const int limit = 5;
            var request = new OffsetLimitFields()
            {
                Offset = "food",
                Limit = limit
            };

            // Act
            var response = Api[apiName].GetCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("food"));
        }

        [Test]
        public void Categories_Offset_Not_Exisiting([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new OffsetLimitFields() { Offset = "qweqweqwe" };

            // Act
            var response = Api[apiName].GetCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
        }


        [Test]
        public void Categories_Search([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("ru");

            // Act
            var response = Api[apiName].SearchCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        public void Categories_Search_Invalid_Query([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("qwerqwerqwerqwerqwerqwerqwerqwer");

            // Act
            var response = Api[apiName].SearchCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        public void Categories_Search_Short_Query([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("f");

            // Act
            var response = Api[apiName].SearchCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Query should have at least 2 characters"));
        }

        [Test]
        public void Categories_Search_Empty_Query([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest(" ");

            // Act
            var response = Api[apiName].SearchCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("This field may not be blank."));
        }

        [Test]
        public void Categories_Search_Offset_Limit([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            const int limit = 5;
            var request = new SearchWithQueryRequest("bit")
            {
                Offset = "bitcoin",
                Limit = limit
            };

            // Act
            var response = Api[apiName].SearchCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("bitcoin"));
        }

        [Test]
        public void Categories_Search_Offset_Not_Exisiting([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("life") { Offset = "qweqweqwe" };

            // Act
            var response = Api[apiName].SearchCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Category used for offset was not found"));
        }

        [Test]
        public void Categories_Search_With_User([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("lif");

            // Act
            var response = Api[apiName].SearchCategories(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test, Sequential]
        public void UserProfile(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("thecryptofiend", "phoenix")] string user)
        {
            // Arrange
            var request = new UserProfileRequest(user);

            // Act
            var response = Api[apiName].GetUserProfile(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
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
            Assert.That(response.Result.CurrentUser, Is.Not.Null);
            Assert.That(response.Result.ProfileImage, Is.Not.Null);
            Assert.That(response.Result.HasFollowed, Is.Not.Null);
            Assert.That(response.Result.EstimatedBalance, Is.Not.Null);
            Assert.That(response.Result.Created, Is.Not.Null);
            Assert.That(response.Result.Name, Is.Not.Null);
            Assert.That(response.Result.About, Is.Not.Null);
            Assert.That(response.Result.Location, Is.Not.Null);
            Assert.That(response.Result.Website, Is.Not.Null);
        }

        [Test]
        public void UserProfile_Invalid_Username([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new UserProfileRequest("qweqweqwe");

            // Act
            var response = Api[apiName].GetUserProfile(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("User not found"));
        }

        [Test, Sequential]
        public void UserProfile_With_User(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("thecryptofiend", "phoenix")] string user)
        {
            // Arrange
            var request = new UserProfileRequest(user) { Login = user };

            // Act
            var response = Api[apiName].GetUserProfile(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
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
            Assert.That(response.Result.CurrentUser, Is.Not.Null);
            Assert.That(response.Result.ProfileImage, Is.Not.Null);
            Assert.That(response.Result.HasFollowed, Is.Not.Null);
            Assert.That(response.Result.EstimatedBalance, Is.Not.Null);
            Assert.That(response.Result.Created, Is.Not.Null);
            Assert.That(response.Result.Name, Is.Not.Null);
            Assert.That(response.Result.About, Is.Not.Null);
            Assert.That(response.Result.Location, Is.Not.Null);
            Assert.That(response.Result.Website, Is.Not.Null);
        }

        [Test]
        public void UserFriends_Following([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserFriendsRequest(user.Login, FriendsType.Following);

            // Act
            var response = Api[apiName].GetUserFriends(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Null);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Null);
            Assert.That(response.Result.Results.First().Reputation, Is.Not.Null);
            Assert.That(response.Result.Results.First().HasFollowed, Is.False);
            var noHasFollowTrueWithoutUser = response.Result.Results.Any(x => x.HasFollowed == true);
            Assert.That(noHasFollowTrueWithoutUser, Is.False);
        }

        [Test]
        public void UserFriends_Followers([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserFriendsRequest(user.Login, FriendsType.Followers);

            // Act
            var response = Api[apiName].GetUserFriends(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Null);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Null);
            Assert.That(response.Result.Results.First().Reputation, Is.Not.Null);
            Assert.That(response.Result.Results.First().HasFollowed, Is.False);
            var noHasFollowTrueWithoutUser = response.Result.Results.Any(x => x.HasFollowed == true);
            Assert.That(noHasFollowTrueWithoutUser, Is.False);
        }

        [Test]
        public void UserFriends_Followers_Invalid_Username([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserFriendsRequest(user.Login + "x", FriendsType.Followers);

            // Act
            var response = Api[apiName].GetUserFriends(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test, Sequential]
        public void UserFriends_Followers_Offset_Limit(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("vowestdream", "pmartynov")] string offset)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserFriendsRequest(user.Login, FriendsType.Followers);
            request.Offset = offset;
            request.Limit = 1;

            // Act
            var response = Api[apiName].GetUserFriends(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.EqualTo(offset));
            Assert.That(response.Result.Results.Count == 1);
        }

        [Test]
        public void UserFriends_Followers_With_User([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new UserFriendsRequest(user.Login, FriendsType.Followers) { Login = user.Login };

            // Act
            var response = Api[apiName].GetUserFriends(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var someResponsesAreHasFollowTrue = response.Result.Results.Any(x => x.HasFollowed == true);
            Assert.That(someResponsesAreHasFollowTrue, Is.True);
        }

        [Test, Sequential]
        public void GetPostInfo(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("/steepshot/@joseph.kalu/cat636416737569422613-2017-09-22-10-42-38", "/steepshot/@joseph.kalu/cat636416737747907631-2017-09-22-10-42-56")] string url)
        {
            // Arrange
            var request = new NamedInfoRequest(url);
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = Api[apiName].GetPostInfo(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Body, Is.Not.Empty);
            Assert.That(response.Result.Title, Is.Not.Empty);
            Assert.That(response.Result.Url, Is.Not.Empty);
            Assert.That(response.Result.Category, Is.Not.Empty);
            Assert.That(response.Result.Author, Is.Not.Empty);
            Assert.That(response.Result.Avatar, Is.Not.Null);
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

        [Test, Sequential]
        public void GetPostInfo_With_User(
            [Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName,
            [Values("spam/@joseph.kalu/test-post-127", "@joseph.kalu/cat636281384922864910")] string url)
        {
            // Arrange
            UserInfo user = Users[apiName];
            var request = new NamedInfoRequest(url) { Login = user.Login };
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = Api[apiName].GetPostInfo(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Body, Is.Not.Empty);
            Assert.That(response.Result.Title, Is.Not.Empty);
            Assert.That(response.Result.Url, Is.Not.Empty);
            Assert.That(response.Result.Category, Is.Not.Empty);
            Assert.That(response.Result.Author, Is.Not.Empty);
            Assert.That(response.Result.Avatar, Is.Not.Null);
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
        public void GetPostInfo_Invalid_Url([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new NamedInfoRequest("spam/@joseph.kalu/qweqeqwqweqweqwe");

            // Act
            var response = Api[apiName].GetPostInfo(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Wrong identifier."));
        }

        [Test]
        public void Upload_Empty_Photo([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new UploadImageRequest(Users[apiName], "title", new byte[0], new[] { "cat1", "cat2", "cat3", "cat4" });

            // Act
            var response = Api[apiName].UploadWithPrepare(request, CancellationToken.None).Result;
            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Upload a valid image. The file you uploaded was either not an image or a corrupted image."));

            //var response = Api[apiName].Upload(request, CancellationToken.None).Result;

            //// Assert
            //AssertResult(response);
            //Assert.That(response.Error.Message.Contains("Upload a valid image. The file you uploaded was either not an image or a corrupted image."));
        }

        [Test]
        public void Upload_Tags_Greater_Than_4([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var file = File.ReadAllBytes(GetTestImagePath());
            var request = new UploadImageRequest(Users[apiName], "cat", file, new[] { "cat1", "cat2", "cat3", "cat4", "cat5" });

            // Act
            var response = Api[apiName].UploadWithPrepare(request, CancellationToken.None).Result;
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("The number of tags should not be more than 4. Please remove a couple of tags and try again."));

            //var response = Api[apiName].Upload(request, CancellationToken.None).Result;

            //// Assert
            //AssertResult(response);
            //Assert.That(response.Error.Message.Contains("The number of tags should not be more than 4. Please remove a couple of tags and try again."));
        }

        [Test]
        public void User_Search([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("aar");

            // Act
            var response = Api[apiName].SearchUser(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
        }

        [Test]
        public void User_Search_Invalid_Query([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("qwerqwerqwerqwerqwerqwerqwerqwer");

            // Act
            var response = Api[apiName].SearchUser(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        public void User_Search_Short_Query([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("fo");

            // Act
            var response = Api[apiName].SearchUser(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Query should have at least 3 characters"));
        }

        [Test]
        public void User_Search_Empty_Query([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest(" ");

            // Act
            var response = Api[apiName].SearchUser(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("This field may not be blank."));
        }

        [Test]
        public void User_Search_Offset_Limit([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            const int limit = 3;
            var request = new SearchWithQueryRequest("bit")
            {
                Offset = "abit",
                Limit = limit
            };

            // Act
            var response = Api[apiName].SearchUser(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.EqualTo("abit"));
        }

        [Test]
        public void User_Search_Offset_Not_Exisiting([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryRequest("aar") { Offset = "qweqweqwe" };

            // Act
            var response = Api[apiName].SearchUser(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Username used for offset was not found"));
        }

        [Test]
        public void User_Exists_Check_Valid_Username([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new UserExistsRequests("pmartynov");

            // Act
            var response = Api[apiName].UserExistsCheck(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.True(response.Result.Exists);
        }

        [Test]
        public void User_Exists_Check_Invalid_Username([Values(KnownChains.Steem, KnownChains.Golos)] KnownChains apiName)
        {
            // Arrange
            var request = new UserExistsRequests("pmartynov123");

            // Act
            var response = Api[apiName].UserExistsCheck(request, CancellationToken.None).Result;

            // Assert
            AssertResult(response);
            Assert.False(response.Result.Exists);
        }

        [Test]
        public void CancelationTest()
        {
            // Arrange
            // Act
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var request = new SearchWithQueryRequest("aar");
                var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                var operationResult = Api[KnownChains.Steem].SearchUser(request, cts.Token).Result;
            });

            // Assert
            Assert.That(ex.InnerException.Message, Is.EqualTo("A task was canceled."));
        }
    }
}