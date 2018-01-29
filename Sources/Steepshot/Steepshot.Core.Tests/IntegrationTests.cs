using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class IntegrationTests : BaseTests
    {
        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Login_With_Posting_Key_Invalid_Credentials(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            user.Login += "x";
            user.PostingKey += "x";
            var request = new AuthorizedModel(user);

            // Act
            var response = await Api[apiName].LoginWithPostingKey(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Invalid private posting key.")
                        || response.Error.Message.Contains("Invalid posting key.")
                        || response.Error.Message.Contains(Localization.Errors.WrongPrivatePostingKey));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Login_With_Posting_Key_Wrong_PostingKey(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            user.PostingKey += "x";
            var request = new AuthorizedModel(user);

            // Act
            var response = await Api[apiName].LoginWithPostingKey(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Invalid private posting key.")
                        || response.Error.Message.Contains("Invalid posting key.")
                        || response.Error.Message.Contains(Localization.Errors.WrongPrivatePostingKey));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Login_With_Posting_Key_Wrong_Username(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            user.Login += "x";
            var request = new AuthorizedModel(user);

            // Act
            var response = await Api[apiName].LoginWithPostingKey(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Invalid private posting key.")
                        || response.Error.Message.Contains("Invalid posting key.")
                        || response.Error.Message.Contains(Localization.Errors.WrongPrivatePostingKey));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserPosts(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserPostsModel(user.Login);
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = await Api[apiName].GetUserPosts(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserPosts_Invalid_Username(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserPostsModel(user.Login + "x");

            // Act
            var response = await Api[apiName].GetUserPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Cannot get posts for this username"));
        }

        [Test]
        [TestCase(KnownChains.Steem, "/steepshot/@joseph.kalu/cat636416737569422613-2017-09-22-10-42-38")]
        [TestCase(KnownChains.Golos, "/steepshot/@joseph.kalu/cat636416737747907631-2017-09-22-10-42-56")]
        public async Task UserPosts_Offset_Limit(KnownChains apiName, string offset)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserPostsModel(user.Login);
            request.Offset = offset;
            request.Limit = 3;
            request.ShowLowRated = true;
            request.ShowNsfw = true;

            // Act
            var response = await Api[apiName].GetUserPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserPosts_With_User_Some_Votes_True(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserPostsModel(user.Login) { Login = user.Login };
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = await Api[apiName].GetUserPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.True);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserPosts_Without_User_All_Votes_False(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserPostsModel(user.Login);

            // Act
            var response = await Api[apiName].GetUserPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserRecentPosts(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new CensoredNamedRequestWithOffsetLimitModel
            {
                Login = user.Login
            };

            // Act
            var response = await Api[apiName].GetUserRecentPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserRecentPosts_Offset_Limit(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new CensoredNamedRequestWithOffsetLimitModel
            {
                Login = user.Login
            };
            var posts = await Api[apiName].GetUserRecentPosts(request, CancellationToken.None);
            request.Offset = posts.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = await Api[apiName].GetUserRecentPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results.First().Body, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_Top(KnownChains apiName)
        {
            // Arrange
            var request = new PostsModel(PostType.Top);

            // Act
            var response = await Api[apiName].GetPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_Top_Limit_Default(KnownChains apiName)
        {
            // Arrange
            const int defaultLimit = 20;
            var request = new PostsModel(PostType.Top);

            // Act
            var response = await Api[apiName].GetPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_Hot_Offset_Limit(KnownChains apiName)
        {
            // Arrange
            var request = new PostsModel(PostType.Hot);
            var posts = await Api[apiName].GetPosts(request, CancellationToken.None);
            request.Offset = posts.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = await Api[apiName].GetPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_Top_With_User(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new PostsModel(PostType.Top) { Login = user.Login };

            // Act
            var response = await Api[apiName].GetPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_Hot(KnownChains apiName)
        {
            // Arrange
            var request = new PostsModel(PostType.Hot);

            // Act
            var response = await Api[apiName].GetPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_New(KnownChains apiName)
        {
            // Arrange
            var request = new PostsModel(PostType.New);

            // Act
            var response = await Api[apiName].GetPosts(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category(KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryModel(PostType.Top, category);

            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_Invalid_Name(KnownChains apiName)
        {
            // Arrange
            var request = new PostsByCategoryModel(PostType.Top, "asdas&^@dsad__sa@@d sd222f_f");

            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.IsTrue(response.Error.Code == 404);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_Not_Existing_Name(KnownChains apiName)
        {
            // Arrange
            var request = new PostsByCategoryModel(PostType.Top, "qweqweqweqewqwqweqe");

            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_Empty_Name(KnownChains apiName)
        {
            // Arrange
            var request = new PostsByCategoryModel(PostType.Top, "");

            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("The Category field is required."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_Hot(KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryModel(PostType.Hot, category);

            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_New(KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryModel(PostType.New, category);

            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var postsWithoutCategoryInTags = response.Result.Results.Where(x => !x.Tags.Contains(category));
            var postShouldHaveCategoryInCategory = postsWithoutCategoryInTags.Any(x => !x.Category.Equals(category));
            Assert.That(postShouldHaveCategoryInCategory, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_Offset_Limit(KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var request = new PostsByCategoryModel(PostType.Top, category);
            var posts = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);
            request.Offset = posts.Result.Results.First().Url;
            request.Limit = 5;

            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.Where(x => !x.Tags.Contains(category)), Is.Empty);
            Assert.That(response.Result.Results.Count, Is.EqualTo(request.Limit));
            Assert.That(response.Result.Results.First().Url, Is.EqualTo(request.Offset));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_With_User(KnownChains apiName)
        {
            var category = "steepshot";
            // Arrange
            var user = Users[apiName];
            var request = new PostsByCategoryModel(PostType.Top, category) { Login = user.Login };
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = await Api[apiName].GetPostsByCategory(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Vote_Up_Already_Voted(KnownChains apiName)
        {
            var user = Users[apiName];
            var userPostsRequest = new CensoredNamedRequestWithOffsetLimitModel();
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.Login = user.Login;
            var posts = await Api[apiName].GetUserRecentPosts(userPostsRequest, CancellationToken.None);
            Assert.IsTrue(posts.IsSuccess);
            var postForVote = posts.Result.Results.FirstOrDefault(i => i.Vote == false);
            Assert.IsNotNull(postForVote);

            var request = new VoteModel(Users[apiName], VoteType.Up, postForVote.Url);
            var response = await Api[apiName].Vote(request, CancellationToken.None);
            AssertResult(response);
            Thread.Sleep(2000);

            var response2 = await Api[apiName].Vote(request, CancellationToken.None);
            AssertResult(response2);

            Assert.That(response2.Error.Message.Contains("You have already voted in a similar way.")
                        || response2.Error.Message.Contains("You`ve already liked this post a few times. Please try another one.")
                        || response2.Error.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Error.Message.Contains("Cannot vote again on a comment after payout.")
                        || response2.Error.Message.Contains("Duplicate transaction check failed")
                        || response2.Error.Message.Contains("Vote weight cannot be 0.")
                        || response2.Error.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Vote_Down_Already_Voted(KnownChains apiName)
        {
            // Load last post
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var posts = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            var lastPost = posts.Result.Results.First();

            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Down, lastPost.Url);

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);
            Thread.Sleep(2000);
            var response2 = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You have already voted in a similar way.")
                        || response2.Error.Message.Contains("You`ve already liked this post a few times. Please try another one.")
                        || response2.Error.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Error.Message.Contains("Duplicate transaction check failed")
                        || response2.Error.Message.Contains("Vote weight cannot be 0.")
                        || response2.Error.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Vote_Invalid_Identifier1(KnownChains apiName)
        {
            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Up, "qwe");

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Vote_Invalid_Identifier2(KnownChains apiName)
        {
            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Up, "qwe/qwe");

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Vote_Invalid_Identifier3(KnownChains apiName)
        {
            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Up, "qwe/qwe");

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Vote_Invalid_Identifier4(KnownChains apiName)
        {
            var request = new VoteModel(Users[apiName], VoteType.Up, "qwe/@qwe");
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Flag_Up_Already_Flagged(KnownChains apiName)
        {
            // Load last post
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var posts = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            var lastPost = posts.Result.Results.First();

            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Flag, lastPost.Url);

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);
            var response2 = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You have already voted in a similar way.")
                        || response2.Error.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Error.Message.Contains("Duplicate transaction check failed")
                        || response2.Error.Message.Contains("Vote weight cannot be 0.")
                        || response2.Error.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Flag_Down_Already_Flagged(KnownChains apiName)
        {
            // Load last post
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var posts = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            var lastPost = posts.Result.Results.First();

            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Down, lastPost.Url);

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);
            var response2 = await Api[apiName].Vote(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Flag_Invalid_Identifier1(KnownChains apiName)
        {
            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Flag, "qwe");

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Flag_Invalid_Identifier2(KnownChains apiName)
        {
            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Flag, "qwe/qwe");

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Flag_Invalid_Identifier3(KnownChains apiName)
        {
            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Flag, "qwe/qwe");

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Flag_Invalid_Identifier4(KnownChains apiName)
        {
            // Arrange
            var request = new VoteModel(Users[apiName], VoteType.Flag, "qwe/@qwe");

            // Act
            var response = await Api[apiName].Vote(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Incorrect identifier"), response.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem, "@joseph.kalu/cat636203355240074655")]
        [TestCase(KnownChains.Golos, "@joseph.kalu/cat636281384922864910")]
        public async Task Comments(KnownChains apiName, string url)
        {
            // Arrange
            var request = new NamedInfoModel(url);

            // Act
            var response = await Api[apiName].GetComments(request, CancellationToken.None);

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

        [Test]
        [TestCase(KnownChains.Steem, "@steepshot/finally-arrived-steepshot-goes-to-beta-meet-the-updated-open-source-android-app")]
        [TestCase(KnownChains.Golos, "@steepshot/dolgozhdannaya-beta-steepshot-dlya-android-polnostyu-obnovlennoe-prilozhenie-s-otkrytym-iskhodnym-kodom")]
        public async Task Comments_With_User_Check_True_Votes(KnownChains apiName, string url)
        {
            // Arrange
            var user = Users[apiName];
            var request = new NamedInfoModel(url) { Login = user.Login };

            // Act
            var response = await Api[apiName].GetComments(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.True);
        }

        [Test]
        [TestCase(KnownChains.Steem, "@dollarvigilante/could-ethereum-be-made-obsolete-by-the-new-decentralized-smart-contract-platform-eos")]
        [TestCase(KnownChains.Golos, "@siberianshamen/chto-takoe-golos")]
        public async Task Comments_Without_User_Check_False_Votes(KnownChains apiName, string url)
        {
            // Arrange
            var request = new NamedInfoModel(url);

            // Act
            var response = await Api[apiName].GetComments(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Comments_Invalid_Url(KnownChains apiName)
        {
            // Arrange
            var request = new NamedInfoModel("qwe");

            // Act
            var response = await Api[apiName].GetComments(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Wrong identifier."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Comments_Invalid_Url_But_Valid_User(KnownChains apiName)
        {
            // Arrange
            var request = new NamedInfoModel("@asduj/qweqweqweqw");

            // Act
            var response = await Api[apiName].GetComments(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Wrong identifier."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task CreateComment_20_Seconds_Delay(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            var userPostsResponse = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);
            var lastPost = userPostsResponse.Result.Results.First();
            var body = $"Test comment {DateTime.Now:G}";
            var createCommentModel = new CreateOrEditCommentModel(Users[apiName], lastPost, body, AppSettings.AppInfo);

            // Act
            var response1 = await Api[apiName].CreateOrEditComment(createCommentModel, CancellationToken.None);
            var response2 = await Api[apiName].CreateOrEditComment(createCommentModel, CancellationToken.None);

            // Assert
            AssertResult(response1);
            AssertResult(response2);
            Assert.That(response2.Error.Message.Contains("You may only comment once every 20 seconds.") || response2.Error.Message.Contains("Duplicate transaction check failed"), response2.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task EditCommentTest(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            var userPostsResponse = await Api[apiName].GetUserPosts(userPostsRequest, CancellationToken.None);

            var post = userPostsResponse.Result.Results.FirstOrDefault(i => i.Children > 0);
            Assert.IsNotNull(post);
            var namedRequest = new NamedInfoModel(post.Url);
            var comments = await Api[apiName].GetComments(namedRequest, CancellationToken.None);
            var comment = comments.Result.Results.FirstOrDefault(i => i.Author.Equals(user.Login));
            Assert.IsNotNull(comment);

            var editCommentRequest = new CreateOrEditCommentModel(user, post, comment, comment.Body += $" edited {DateTime.Now}", AppSettings.AppInfo);

            var result = await Api[apiName].CreateOrEditComment(editCommentRequest, CancellationToken.None);
            AssertResult(result);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories(KnownChains apiName)
        {
            // Arrange
            var request = new OffsetLimitModel();

            // Act
            var response = await Api[apiName].GetCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Offset_Limit(KnownChains apiName)
        {
            // Arrange
            const int limit = 5;
            var request = new OffsetLimitModel()
            {
                Offset = "food",
                Limit = limit
            };

            // Act
            var response = await Api[apiName].GetCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("food"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Offset_Not_Exisiting(KnownChains apiName)
        {
            // Arrange
            var request = new OffsetLimitModel() { Offset = "qweqweqwe" };

            // Act
            var response = await Api[apiName].GetCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
        }


        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("ru");

            // Act
            var response = await Api[apiName].SearchCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_Invalid_Query(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("qwerqwerqwerqwerqwerqwerqwerqwer");

            // Act
            var response = await Api[apiName].SearchCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_Short_Query(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("f");

            // Act
            var response = await Api[apiName].SearchCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Query should have at least 2 characters"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_Empty_Query(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel(" ");

            // Act
            var response = await Api[apiName].SearchCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("The Query field is required."), response.Error.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_Offset_Limit(KnownChains apiName)
        {
            // Arrange
            const int limit = 5;
            var request = new SearchWithQueryModel("bit")
            {
                Offset = "bitcoin",
                Limit = limit
            };

            // Act
            var response = await Api[apiName].SearchCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.EqualTo("bitcoin"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_Offset_Not_Exisiting(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("life") { Offset = "qweqweqwe" };

            // Act
            var response = await Api[apiName].SearchCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Category used for offset was not found"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_With_User(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("lif");

            // Act
            var response = await Api[apiName].SearchCategories(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Any());
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Name, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem, "thecryptofiend")]
        [TestCase(KnownChains.Golos, "phoenix")]
        public async Task UserProfile(KnownChains apiName, string user)
        {
            // Arrange
            var request = new UserProfileModel(user);

            // Act
            var response = await Api[apiName].GetUserProfile(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserProfile_Invalid_Username(KnownChains apiName)
        {
            // Arrange
            var request = new UserProfileModel("qweqweqwe");

            // Act
            var response = await Api[apiName].GetUserProfile(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("User not found"));
        }

        [Test]
        [TestCase(KnownChains.Steem, "thecryptofiend")]
        [TestCase(KnownChains.Golos, "phoenix")]
        public async Task UserProfile_With_User(KnownChains apiName, string user)
        {
            // Arrange
            var request = new UserProfileModel(user) { Login = user };

            // Act
            var response = await Api[apiName].GetUserProfile(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserFriends_Following(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserFriendsModel(user.Login, FriendsType.Following);

            // Act
            var response = await Api[apiName].GetUserFriends(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserFriends_Followers(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserFriendsModel(user.Login, FriendsType.Followers);

            // Act
            var response = await Api[apiName].GetUserFriends(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserFriends_Followers_Invalid_Username(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserFriendsModel(user.Login + "x", FriendsType.Followers);

            // Act
            var response = await Api[apiName].GetUserFriends(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Code == 404);
        }

        [Test]
        [TestCase(KnownChains.Steem, "vowestdream")]
        [TestCase(KnownChains.Golos, "pmartynov")]
        public async Task UserFriends_Followers_Offset_Limit(KnownChains apiName, string offset)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserFriendsModel(user.Login, FriendsType.Followers);
            request.Offset = offset;
            request.Limit = 1;

            // Act
            var response = await Api[apiName].GetUserFriends(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.EqualTo(offset));
            Assert.That(response.Result.Results.Count == 1);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserFriends_Followers_With_User(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var request = new UserFriendsModel(user.Login, FriendsType.Followers) { Login = user.Login };

            // Act
            var response = await Api[apiName].GetUserFriends(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            var someResponsesAreHasFollowTrue = response.Result.Results.Any(x => x.HasFollowed == true);
            Assert.That(someResponsesAreHasFollowTrue, Is.True);
        }

        [Test]
        [TestCase(KnownChains.Steem, "/steepshot/@joseph.kalu/cat636416737569422613-2017-09-22-10-42-38")]
        [TestCase(KnownChains.Golos, "/steepshot/@joseph.kalu/cat636416737747907631-2017-09-22-10-42-56")]
        public async Task GetPostInfo(KnownChains apiName, string url)
        {
            // Arrange
            var request = new NamedInfoModel(url);
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = await Api[apiName].GetPostInfo(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem, "spam/@joseph.kalu/test-post-127")]
        [TestCase(KnownChains.Golos, "@joseph.kalu/cat636281384922864910")]
        public async Task GetPostInfo_With_User(KnownChains apiName, string url)
        {
            // Arrange
            var user = Users[apiName];
            var request = new NamedInfoModel(url) { Login = user.Login };
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            // Act
            var response = await Api[apiName].GetPostInfo(request, CancellationToken.None);

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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task GetPostInfo_Invalid_Url(KnownChains apiName)
        {
            // Arrange
            var request = new NamedInfoModel("spam/@joseph.kalu/qweqeqwqweqweqwe");

            // Act
            var response = await Api[apiName].GetPostInfo(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Wrong identifier."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Upload_Empty_Photo(KnownChains apiName)
        {
            // Arrange
            var request = new UploadMediaModel(Users[apiName], new MemoryStream(), ".jpg");

            // Act
            var response = await Api[apiName].UploadMedia(request, CancellationToken.None);
            // Assert
            AssertResult(response);
            Assert.IsTrue(string.Equals(response.Error.Message, Localization.Errors.EmptyFileField));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("aar");

            // Act
            var response = await Api[apiName].SearchUser(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search_Invalid_Query(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("qwerqwerqwerqwerqwerqwerqwerqwer");

            // Act
            var response = await Api[apiName].SearchUser(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search_Short_Query(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("fo");

            // Act
            var response = await Api[apiName].SearchUser(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Query should have at least 3 characters"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search_Empty_Query(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel(" ");

            // Act
            var response = await Api[apiName].SearchUser(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("The Query field is required."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search_Offset_Limit(KnownChains apiName)
        {
            // Arrange
            const int limit = 3;
            var request = new SearchWithQueryModel("bit")
            {
                Offset = "abit",
                Limit = limit
            };

            // Act
            var response = await Api[apiName].SearchUser(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results.Count, Is.EqualTo(limit));
            Assert.That(response.Result.Results, Is.Not.Empty);
            Assert.That(response.Result.Results.First().Author, Is.EqualTo("abit"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search_Offset_Not_Exisiting(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("aar") { Offset = "qweqweqwe" };

            // Act
            var response = await Api[apiName].SearchUser(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Error.Message.Contains("Username used for offset was not found"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Exists_Check_Valid_Username(KnownChains apiName)
        {
            // Arrange
            var request = new UserExistsModel("pmartynov");

            // Act
            var response = await Api[apiName].UserExistsCheck(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.True(response.Result.Exists);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Exists_Check_Invalid_Username(KnownChains apiName)
        {
            // Arrange
            var request = new UserExistsModel("pmartynov123");

            // Act
            var response = await Api[apiName].UserExistsCheck(request, CancellationToken.None);

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
                var request = new SearchWithQueryModel("aar");
                var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                var operationResult = Api[KnownChains.Steem].SearchUser(request, cts.Token).Result;
            });

            // Assert
            Assert.That(ex.InnerException.Message, Is.EqualTo("A task was canceled."));
        }
    }
}