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
        [Test, Sequential]
        public void Login_With_Posting_Key_Invalid_Credentials([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            user.Login += "x";
            user.PostingKey += "x";
            var request = new AuthorizedRequest(user);

            // Act
            var response = Api[name].LoginWithPostingKey(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Invalid private posting key.") ||
                        response.Errors.Contains("Invalid posting key."));
        }

        [Test, Sequential]
        public void Login_With_Posting_Key_Wrong_PostingKey([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            user.PostingKey += "x";
            var request = new AuthorizedRequest(user);

            // Act
            var response = Api[name].LoginWithPostingKey(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Invalid private posting key.") ||
                        response.Errors.Contains("Invalid posting key."));
        }

        [Test, Sequential]
        public void Login_With_Posting_Key_Wrong_Username([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            user.Login += "x";
            var request = new AuthorizedRequest(user);

            // Act
            var response = Api[name].LoginWithPostingKey(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Invalid private posting key.") ||
                        response.Errors.Contains("Invalid posting key."));
        }

        [Test, Sequential]
        public void UserPosts([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserPostsRequest(user.Login);

            // Act
            var response = Api[name].GetUserPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Empty);
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

        [Test, Sequential]
        public void UserPosts_Invalid_Username([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserPostsRequest(user.Login + "x");

            // Act
            var response = Api[name].GetUserPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Cannot get posts for this username"));
        }

        [Test, Sequential]
        public void UserPosts_Offset_Limit(
        [Values("Steem", "Golos")] string name,
        [Values("/cat1/@joseph.kalu/cat636203389144533548", "/cat1/@joseph.kalu/cat636281384922864910")] string offset)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserPostsRequest(user.Login);
            request.Offset = offset;
            request.Limit = 3;

            // Act
            var response = Api[name].GetUserPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
            Assert.That(response.Result.Count, Is.EqualTo(request.Limit));
        }

        [Test, Sequential]
        public void UserPosts_With_SessionId_Some_Votes_True([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserPostsRequest(user.Login) { Login = user.Login };

            // Act
            var response = Api[name].GetUserPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.True);
        }

        [Test, Sequential]
        public void UserPosts_Without_SessionId_All_Votes_False([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserPostsRequest(user.Login);

            // Act
            var response = Api[name].GetUserPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.False);
        }

        [Test, Sequential]
        public void UserRecentPosts([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new CensoredPostsRequests
            {
                Login = user.Login
            };

            // Act
            var response = Api[name].GetUserRecentPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
        }

        [Test, Sequential]
        public void UserRecentPosts_Offset_Limit([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new CensoredPostsRequests
            {
                Login = user.Login
            };
            request.Offset = Api[name].GetUserRecentPosts(request).Result.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = Api[name].GetUserRecentPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
            Assert.That(response.Result.Count, Is.EqualTo(request.Limit));
        }

        [Test, Sequential]
        public void Posts_Top([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsRequest(PostType.Top);

            // Act
            var response = Api[name].GetPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test, Sequential]
        public void Posts_Top_Limit_Default([Values("Steem", "Golos")] string name)
        {
            // Arrange
            const int defaultLimit = 20;
            var request = new PostsRequest(PostType.Top);

            // Act
            var response = Api[name].GetPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.EqualTo(defaultLimit));
        }

        [Test, Sequential]
        public void Posts_Hot_Offset_Limit([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsRequest(PostType.Hot);
            request.Offset = Api[name].GetPosts(request).Result.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = Api[name].GetPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Offset, Is.Not.Empty);
            Assert.That(response.Result.Count > 0);
            Assert.That(request.Limit, Is.EqualTo(response.Result.Count));
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test, Sequential]
        public void Posts_Top_With_SessionId([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new PostsRequest(PostType.Top) { Login = user.Login };

            // Act
            var response = Api[name].GetPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Offset, Is.Not.Empty);
            Assert.That(response.Result.Count > 0);
        }

        [Test, Sequential]
        public void Posts_Hot([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsRequest(PostType.Hot);

            // Act
            var response = Api[name].GetPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test, Sequential]
        public void Posts_New([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsRequest(PostType.New);

            // Act
            var response = Api[name].GetPosts(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test, Sequential]
        public void Posts_By_Category([Values("Steem", "Golos")] string name, [Values("food", "ru--golos")] string category)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, category);

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test, Sequential]
        public void Posts_By_Category_Invalid_Name([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, "asdas&^@dsad__sa@@d sd222f_f");

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Not Found"));
        }

        [Test, Sequential]
        public void Posts_By_Category_Not_Existing_Name([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, "qweqweqweqewqwqweqe");

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test, Sequential]
        public void Posts_By_Category_Empty_Name([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, "");

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Not Found"));
        }

        [Test, Sequential]
        public void Posts_By_Category_Hot([Values("Steem", "Golos")] string name, [Values("food", "ru--golos")] string category)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Hot, category);

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test, Sequential]
        public void Posts_By_Category_New([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.New, "food");

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains("food"));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals("food"));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test, Sequential]
        public void Posts_By_Category_Offset_Limit([Values("Steem", "Golos")] string name, [Values("food", "ru--golos")] string category)
        {
            // Arrange
            var request = new PostsByCategoryRequest(PostType.Top, category);
            request.Offset = Api[name].GetPostsByCategory(request).Result.Result.Results.First().Url;
            request.Limit = 5;

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Where(x => !x.Tags.Contains(category)), Is.Empty);
            Assert.That(response.Result.Results.Count, Is.EqualTo(request.Limit));
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test, Sequential]
        public void Posts_By_Category_With_SessionId([Values("Steem", "Golos")] string name, [Values("food", "ru--golos")] string category)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new PostsByCategoryRequest(PostType.Top, category) { Login = user.Login };

            // Act
            var response = Api[name].GetPostsByCategory(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test, Sequential]
        public void Vote_Up_Already_Voted([Values("Steem", "Golos")] string name)
        {
            // Load last post
            UserInfo user = Users[name];
            var userPostsRequest = new UserPostsRequest(user.Login);
            var lastPost = Api[name].GetUserPosts(userPostsRequest).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Up, lastPost.Url);

            // Act
            var response = Api[name].Vote(request).Result;
            Thread.Sleep(2000);
            var response2 = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response2);
            Assert.That(response2.Errors.Contains("You have already voted in a similar way.")
                        || response2.Errors.Contains("Can only vote once every 3 seconds")
                        || response2.Errors.Contains("Duplicate transaction check failed")
                        || response2.Errors.Contains("Vote weight cannot be 0.")
                        || response2.Errors.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), string.Join(Environment.NewLine, response2.Errors));
        }

        [Test, Sequential]
        public void Vote_Down_Already_Voted([Values("Steem", "Golos")] string name)
        {
            // Load last post
            UserInfo user = Users[name];
            var userPostsRequest = new UserPostsRequest(user.Login);
            var lastPost = Api[name].GetUserPosts(userPostsRequest).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Down, lastPost.Url);

            // Act
            var response = Api[name].Vote(request).Result;
            Thread.Sleep(2000);
            var response2 = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response2);
            Assert.That(response2.Errors.Contains("You have already voted in a similar way")
                        || response2.Errors.Contains("Can only vote once every 3 seconds")
                        || response2.Errors.Contains("Duplicate transaction check failed")
                        || response2.Errors.Contains("Vote weight cannot be 0.")
                        || response2.Errors.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), string.Join(Environment.NewLine, response2.Errors));
        }

        [Test, Sequential]
        public void Vote_Invalid_Identifier1([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Up, "qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test, Sequential]
        public void Vote_Invalid_Identifier2([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Up, "qwe/qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test, Sequential]
        public void Vote_Invalid_Identifier3([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Up, "qwe/qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test, Sequential]
        public void Vote_Invalid_Identifier4([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Up, "qwe/@qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"));
        }

        [Test, Sequential]
        public void Flag_Up_Already_Flagged([Values("Steem", "Golos")] string name)
        {
            // Load last post
            UserInfo user = Users[name];
            var userPostsRequest = new UserPostsRequest(user.Login);
            var lastPost = Api[name].GetUserPosts(userPostsRequest).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Flag, lastPost.Url);

            // Act
            var response = Api[name].Vote(request).Result;
            var response2 = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response2);
            Assert.That(response2.Errors.Contains("You have already voted in a similar way")
                        || response2.Errors.Contains("Can only vote once every 3 seconds")
                        || response2.Errors.Contains("Duplicate transaction check failed")
                        || response2.Errors.Contains("Vote weight cannot be 0.")
                        || response2.Errors.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), string.Join(Environment.NewLine, response2.Errors));
        }

        [Test, Sequential]
        public void Flag_Down_Already_Flagged([Values("Steem", "Golos")] string name)
        {
            // Load last post
            UserInfo user = Users[name];
            var userPostsRequest = new UserPostsRequest(user.Login);
            var lastPost = Api[name].GetUserPosts(userPostsRequest).Result.Result.Results.First();

            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Down, lastPost.Url);

            // Act
            var response = Api[name].Vote(request).Result;
            var response2 = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response2);
            AssertResult(response2);
            Assert.That(response2.Errors.Contains("You have already voted in a similar way")
                        || response2.Errors.Contains("Can only vote once every 3 seconds")
                        || response2.Errors.Contains("Duplicate transaction check failed")
                        || response2.Errors.Contains("Vote weight cannot be 0.")
                        || response2.Errors.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), string.Join(Environment.NewLine, response2.Errors));
        }

        [Test, Sequential]
        public void Flag_Invalid_Identifier1([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Flag, "qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"), string.Join(Environment.NewLine, response.Errors));
        }

        [Test, Sequential]
        public void Flag_Invalid_Identifier2([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Flag, "qwe/qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"), string.Join(Environment.NewLine, response.Errors));
        }

        [Test, Sequential]
        public void Flag_Invalid_Identifier3([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Flag, "qwe/qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"), string.Join(Environment.NewLine, response.Errors));
        }

        [Test, Sequential]
        public void Flag_Invalid_Identifier4([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new VoteRequest(Authenticate(name), VoteType.Flag, "qwe/@qwe");

            // Act
            var response = Api[name].Vote(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Incorrect identifier"), string.Join(Environment.NewLine, response.Errors));
        }

        [Test, Sequential]
        public void Comments([Values("Steem", "Golos")] string name,
                             [Values("@joseph.kalu/cat636203355240074655",
                                     "@joseph.kalu/cat636281384922864910")] string url)
        {
            // Arrange
            var request = new NamedInfoRequest(url);

            // Act
            var response = Api[name].GetComments(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
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
        public void Comments_With_SessionId_Check_True_Votes([Values("Steem", "Golos")] string name,
            [Values("@joseph.kalu/cat636203355240074655", "@joseph.kalu/hi-golos")] string url)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new NamedInfoRequest(url) { Login = user.Login };

            // Act
            var response = Api[name].GetComments(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.True);
        }

        [Test, Sequential]
        public void Comments_Without_SessionId_Check_False_Votes(
        [Values("Steem", "Golos")] string name,
        [Values("@dollarvigilante/could-ethereum-be-made-obsolete-by-the-new-decentralized-smart-contract-platform-eos",
                "@siberianshamen/chto-takoe-golos")] string url)
        {
            // Arrange
            var request = new NamedInfoRequest(url);

            // Act
            var response = Api[name].GetComments(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.False);
        }

        [Test, Sequential]
        public void Comments_Invalid_Url([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new NamedInfoRequest("qwe");

            // Act
            var response = Api[name].GetComments(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
        }

        [Test, Sequential]
        public void Comments_Invalid_Url_But_Valid_User([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new NamedInfoRequest("@asduj/qweqweqweqw");

            // Act
            var response = Api[name].GetComments(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
        }

        [Test, Sequential]
        public void CreateComment_20_Seconds_Delay([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var userPostsRequest = new UserPostsRequest(user.Login);
            var userPostsResponse = Api[name].GetUserPosts(userPostsRequest).Result;
            var lastPost = userPostsResponse.Result.Results.First();
            const string body = "Ллойс!";
            const string title = "Лучший камент ever";
            var createCommentRequest = new CreateCommentRequest(Authenticate(name), lastPost.Url, body, title, AppSettings.AppInfo);

            // Act
            var response1 = Api[name].CreateComment(createCommentRequest).Result;
            var response2 = Api[name].CreateComment(createCommentRequest).Result;

            // Assert
            AssertResult(response1);
            AssertResult(response2);
            Assert.That(response2.Errors.Contains("You may only comment once every 20 seconds.") || response2.Errors.Contains("Duplicate transaction check failed"), string.Join(Environment.NewLine, response2.Errors));
        }

        [Test, Sequential]
        public void Categories([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new OffsetLimitFields();

            // Act
            var response = Api[name].GetCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test, Sequential]
        public void Categories_Offset_Limit([Values("Steem", "Golos")] string name)
        {
            // Arrange
            const int limit = 5;
            var request = new OffsetLimitFields()
            {
                Offset = "food",
                Limit = limit
            };

            // Act
            var response = Api[name].GetCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("food"));
        }

        [Test, Sequential]
        public void Categories_Offset_Not_Exisiting([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new OffsetLimitFields() { Offset = "qweqweqwe" };

            // Act
            var response = Api[name].GetCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount, Is.EqualTo(-1));
            Assert.That(response.Result.Results, Is.Not.Empty);
        }


        [Test, Sequential]
        public void Categories_Search([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("ru");

            // Act
            var response = Api[name].SearchCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount >= 0);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test, Sequential]
        public void Categories_Search_Invalid_Query([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("qwerqwerqwerqwerqwerqwerqwerqwer");

            // Act
            var response = Api[name].SearchCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
            Assert.That(response.Result.Count, Is.EqualTo(0));
            Assert.That(response.Result.TotalCount, Is.EqualTo(0));
        }

        [Test, Sequential]
        public void Categories_Search_Short_Query([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("f");

            // Act
            var response = Api[name].SearchCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Query should have at least 2 characters"));
        }

        [Test, Sequential]
        public void Categories_Search_Empty_Query([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest(" ");

            // Act
            var response = Api[name].SearchCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test, Sequential]
        public void Categories_Search_Offset_Limit([Values("Steem", "Golos")] string name)
        {
            // Arrange
            const int limit = 5;
            var request = new SearchWithQueryRequest("bit")
            {
                Offset = "bitcoin",
                Limit = limit
            };

            // Act
            var response = Api[name].SearchCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.TotalCount > limit);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("bitcoin"));
        }

        [Test, Sequential]
        public void Categories_Search_Offset_Not_Exisiting([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("life") { Offset = "qweqweqwe" };

            // Act
            var response = Api[name].SearchCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Category used for offset was not found"));
        }

        [Test, Sequential]
        public void Categories_Search_With_SessionId([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("lif");

            // Act
            var response = Api[name].SearchCategories(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount >= 0);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test, Sequential]
        public void UserProfile([Values("Steem", "Golos")] string name, [Values("thecryptofiend", "phoenix")] string user)
        {
            // Arrange
            var request = new UserProfileRequest(user);

            // Act
            var response = Api[name].GetUserProfile(request).Result;

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
            Assert.That(response.Result.CurrentUsername, Is.Not.Null);
            Assert.That(response.Result.ProfileImage, Is.Not.Null);
            Assert.That(response.Result.HasFollowed, Is.Not.Null);
            Assert.That(response.Result.EstimatedBalance, Is.Not.Null);
            Assert.That(response.Result.Created, Is.Not.Null);
            Assert.That(response.Result.Name, Is.Not.Null);
            Assert.That(response.Result.About, Is.Not.Null);
            Assert.That(response.Result.Location, Is.Not.Null);
            Assert.That(response.Result.Website, Is.Not.Null);
        }

        [Test, Sequential]
        public void UserProfile_Invalid_Username([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new UserProfileRequest("qweqweqwe");

            // Act
            var response = Api[name].GetUserProfile(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("User not found"));
        }

        [Test, Sequential]
        public void UserProfile_With_SessionId([Values("Steem", "Golos")] string name, [Values("thecryptofiend", "phoenix")] string user)
        {
            // Arrange
            var request = new UserProfileRequest(user) { Login = user };

            // Act
            var response = Api[name].GetUserProfile(request).Result;

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
            Assert.That(response.Result.CurrentUsername, Is.Not.Null);
            Assert.That(response.Result.ProfileImage, Is.Not.Null);
            Assert.That(response.Result.HasFollowed, Is.Not.Null);
            Assert.That(response.Result.EstimatedBalance, Is.Not.Null);
            Assert.That(response.Result.Created, Is.Not.Null);
            Assert.That(response.Result.Name, Is.Not.Null);
            Assert.That(response.Result.About, Is.Not.Null);
            Assert.That(response.Result.Location, Is.Not.Null);
            Assert.That(response.Result.Website, Is.Not.Null);
        }

        [Test, Sequential]
        public void UserFriends_Following([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserFriendsRequest(user.Login, FriendsType.Following);

            // Act
            var response = Api[name].GetUserFriends(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Null);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Null);
            Assert.That(response.Result.Results.First().Reputation, Is.Not.Null);
            Assert.That(response.Result.Results.First().HasFollowed, Is.False);
            var noHasFollowTrueWithoutSessionId = response.Result.Results.Any(x => x.HasFollowed == true);
            Assert.That(noHasFollowTrueWithoutSessionId, Is.False);
        }

        [Test, Sequential]
        public void UserFriends_Followers([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserFriendsRequest(user.Login, FriendsType.Followers);

            // Act
            var response = Api[name].GetUserFriends(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Null);
            Assert.That(response.Result.Results.First().Avatar, Is.Not.Null);
            Assert.That(response.Result.Results.First().Reputation, Is.Not.Null);
            Assert.That(response.Result.Results.First().HasFollowed, Is.False);
            var noHasFollowTrueWithoutSessionId = response.Result.Results.Any(x => x.HasFollowed == true);
            Assert.That(noHasFollowTrueWithoutSessionId, Is.False);
        }

        [Test, Sequential]
        public void UserFriends_Followers_Invalid_Username([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserFriendsRequest(user.Login + "x", FriendsType.Followers);

            // Act
            var response = Api[name].GetUserFriends(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count == 0);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test, Sequential]
        public void UserFriends_Followers_Offset_Limit([Values("Steem", "Golos")] string name, [Values("vowestdream", "pmartynov")] string offset)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserFriendsRequest(user.Login, FriendsType.Followers);
            request.Offset = offset;
            request.Limit = 1;

            // Act
            var response = Api[name].GetUserFriends(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.EqualTo(offset));
            Assert.That(response.Result.Results.Count == 1);
        }

        [Test, Sequential]
        public void UserFriends_Followers_With_SessionId([Values("Steem", "Golos")] string name)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new UserFriendsRequest(user.Login, FriendsType.Followers) { Login = user.Login };

            // Act
            var response = Api[name].GetUserFriends(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.Not.Null);
            Assert.That(response.Result.Offset, Is.Not.Null);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var someResponsesAreHasFollowTrue = response.Result.Results.Any(x => x.HasFollowed == true);
            Assert.That(someResponsesAreHasFollowTrue, Is.True);
        }

        [Test, Sequential]
        public void Terms_Of_Service([Values("Steem", "Golos")] string name)
        {
            // Arrange
            // Act
            var response = Api[name].TermsOfService().Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Text, Is.Not.Empty);
        }

        [Test, Sequential]
        public void GetPostInfo([Values("Steem", "Golos")] string name,
            [Values("spam/@joseph.kalu/test-post-127", "@joseph.kalu/cat636281384922864910")] string url)
        {
            // Arrange
            var request = new NamedInfoRequest(url);

            // Act
            var response = Api[name].GetPostInfo(request).Result;

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
        public void GetPostInfo_With_SessionId([Values("Steem", "Golos")] string name, [Values("spam/@joseph.kalu/test-post-127", "@joseph.kalu/cat636281384922864910")] string url)
        {
            // Arrange
            UserInfo user = Users[name];
            var request = new NamedInfoRequest(url) { Login = user.Login };

            // Act
            var response = Api[name].GetPostInfo(request).Result;

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
        public void GetPostInfo_Invalid_Url([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new NamedInfoRequest("spam/@joseph.kalu/qweqeqwqweqweqwe");

            // Act
            var response = Api[name].GetPostInfo(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Wrong identifier."));
        }

        [Test, Sequential]
        public void Upload_Empty_Photo([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new UploadImageRequest(Authenticate(name), "title", "cat1", "cat2", "cat3", "cat4");

            // Act
            var response = Api[name].Upload(request, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None)).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Upload a valid image. The file you uploaded was either not an image or a corrupted image."));
        }

        [Test, Sequential]
        public void Upload_Tags_Greater_Than_4([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var file = File.ReadAllBytes(GetTestImagePath());
            var request = new UploadImageRequest(Authenticate(name), "cat", file, "cat1", "cat2", "cat3", "cat4", "cat5");

            // Act
            var response = Api[name].Upload(request, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None)).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("The number of tags should not be more than 4. Please remove a couple of tags and try again."));
        }

        [Test, Sequential]
        public void User_Search([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("aar");

            // Act
            var response = Api[name].SearchUser(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count > 0);
            Assert.That(response.Result.TotalCount >= 0);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Username, Is.Not.Empty);
        }

        [Test, Sequential]
        public void User_Search_Invalid_Query([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("qwerqwerqwerqwerqwerqwerqwerqwer");

            // Act
            var response = Api[name].SearchUser(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
            Assert.That(response.Result.Count, Is.EqualTo(0));
            Assert.That(response.Result.TotalCount, Is.EqualTo(0));
        }

        [Test, Sequential]
        public void User_Search_Short_Query([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("fo");

            // Act
            var response = Api[name].SearchUser(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Query should have at least 3 characters"));
        }

        [Test, Sequential]
        public void User_Search_Empty_Query([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest(" ");

            // Act
            var response = Api[name].SearchUser(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("This field may not be blank."));
        }

        [Test, Sequential]
        public void User_Search_Offset_Limit([Values("Steem", "Golos")] string name)
        {
            // Arrange
            const int limit = 3;
            var request = new SearchWithQueryRequest("bit")
            {
                Offset = "abit",
                Limit = limit
            };

            // Act
            var response = Api[name].SearchUser(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.TotalCount >= limit);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Username, Is.EqualTo("abit"));
        }

        [Test, Sequential]
        public void User_Search_Offset_Not_Exisiting([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new SearchWithQueryRequest("aar") { Offset = "qweqweqwe" };

            // Act
            var response = Api[name].SearchUser(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Errors.Contains("Username used for offset was not found"));
        }

        [Test, Sequential]
        public void User_Exists_Check_Valid_Username([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new UserExistsRequests("pmartynov");

            // Act
            var response = Api[name].UserExistsCheck(request).Result;

            // Assert
            AssertResult(response);
            Assert.True(response.Result.Exists);
        }

        [Test, Sequential]
        public void User_Exists_Check_Invalid_Username([Values("Steem", "Golos")] string name)
        {
            // Arrange
            var request = new UserExistsRequests("pmartynov123");

            // Act
            var response = Api[name].UserExistsCheck(request).Result;

            // Assert
            AssertResult(response);
            Assert.False(response.Result.Exists);
        }

        [Test, Sequential]
        public void CancelationTest()
        {
            // Arrange
            // Act
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var request = new SearchWithQueryRequest("aar");
                var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                var operationResult = Api["Steem"].SearchUser(request, cts).Result;
            });

            // Assert
            Assert.That(ex.InnerException.Message, Is.EqualTo("A task was canceled."));
        }
    }
}