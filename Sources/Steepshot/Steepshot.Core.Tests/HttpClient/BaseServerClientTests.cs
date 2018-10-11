using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class BaseServerClientTests : BaseTests
    {
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
            var response = await SteepshotApi[apiName].GetUserPostsAsync(request, CancellationToken.None);

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
            //Assert.That(response.Result.Results.First().NetVotes, Is.Not.Null);
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
            var response = await SteepshotApi[apiName].GetUserPostsAsync(request, CancellationToken.None);

            // Assert
            Assert.That(response.Exception.Message.Contains("Cannot get posts for this username"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserPosts_Offset_Limit(KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserPostsModel(user.Login)
            {
                Limit = 10,
                ShowLowRated = true,
                ShowNsfw = true
            };

            var response = await SteepshotApi[apiName].GetUserPostsAsync(request, CancellationToken.None);
            AssertResult(response);
            Assert.IsTrue(response.Result.Count == request.Limit);
            request.Offset = response.Result.Results[5].Url;

            response = await SteepshotApi[apiName].GetUserPostsAsync(request, CancellationToken.None);

            Assert.IsTrue(response.Result.Results != null);
            var url = response.Result.Results.First().Url;
            Assert.IsFalse(string.IsNullOrEmpty(url));
            Assert.IsTrue(url.Equals(request.Offset));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task UserPosts_With_User_Some_Votes_True(KnownChains apiName)
        {
            var user = Users[apiName];
            var request = new UserPostsModel(user.Login) { Login = user.Login };
            request.ShowNsfw = true;
            request.ShowLowRated = true;
            var voded = false;

            for (var i = 0; i < 10; i++)
            {

                var response = await SteepshotApi[apiName].GetUserPostsAsync(request, CancellationToken.None);
                AssertResult(response);
                voded = response.Result.Results.Any(x => x.Vote);
                if (voded)
                    break;
                request.Offset = response.Result.Results.Last().Url;
            }

            Assert.IsTrue(voded);
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
            var response = await SteepshotApi[apiName].GetUserPostsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetUserRecentPostsAsync(request, CancellationToken.None);

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
            var posts = await SteepshotApi[apiName].GetUserRecentPostsAsync(request, CancellationToken.None);
            request.Offset = posts.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = await SteepshotApi[apiName].GetUserRecentPostsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);

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
            var request = new PostsModel(PostType.Top);

            // Act
            var response = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);

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
            var posts = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);
            request.Offset = posts.Result.Results.First().Url;
            request.Limit = 3;

            // Act
            var response = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

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
            var request = new PostsByCategoryModel(PostType.Top, "asdas&^@dsad__sa@@d sd222f_f");

            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

            Assert.IsTrue(response.Exception.Message.StartsWith("<h1>Not Found</h1>"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_Not_Existing_Name(KnownChains apiName)
        {
            var request = new PostsByCategoryModel(PostType.Top, "qweqweqweqewqwqweqe");

            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

            AssertResult(response);
            Assert.That(response.Result.Results, Is.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Posts_By_Category_Empty_Name(KnownChains apiName)
        {
            var request = new PostsByCategoryModel(PostType.Top, string.Empty);

            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

            Assert.That(response.Exception.Message.StartsWith(nameof(LocalizationKeys.EmptyCategory)));
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
            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

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
            var posts = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);
            request.Offset = posts.Result.Results.First().Url;
            request.Limit = 5;

            // Act
            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetPostsByCategoryAsync(request, CancellationToken.None);

            // Assert
            AssertResult(response);
            Assert.That(response.Result.Results, Is.Not.Empty);
        }

        [Test]
        [TestCase(KnownChains.Steem, "@joseph.kalu/cat636203355240074655")]
        [TestCase(KnownChains.Golos, "@joseph.kalu/cat636281384922864910")]
        public async Task Comments(KnownChains apiName, string url)
        {
            // Arrange
            var request = new NamedInfoModel(url);

            // Act
            var response = await SteepshotApi[apiName].GetCommentsAsync(request, CancellationToken.None);

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
            //Assert.That(response.Result.Results.First().NetVotes, Is.Not.Null);
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
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Comments_With_User_Check_True_Votes(KnownChains apiName)
        {
            var request = new PostsModel(PostType.Hot);
            var posts = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);
            var isVoted = false;
            var user = Users[apiName];
            foreach (var post in posts.Result.Results.Where(i => i.Children > 0))
            {
                var infoModel = new NamedInfoModel(post.Url) { Login = user.Login };

                var response = await SteepshotApi[apiName].GetCommentsAsync(infoModel, CancellationToken.None);
                AssertResult(response);
                isVoted = response.Result.Results.Any(x => x.Vote);
                if (isVoted)
                    break;
            }
            Assert.IsTrue(isVoted);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Comments_Without_User_Check_False_Votes(KnownChains apiName)
        {
            var request = new PostsModel(PostType.Hot);
            var posts = await SteepshotApi[apiName].GetPostsAsync(request, CancellationToken.None);


            var infoModel = new NamedInfoModel(posts.Result.Results.First(i => i.Children > 0).Url);

            var response = await SteepshotApi[apiName].GetCommentsAsync(infoModel, CancellationToken.None);

            AssertResult(response);
            Assert.That(response.Result.Results.Where(x => x.Vote).Any, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Comments_Invalid_Url(KnownChains apiName)
        {
            var request = new NamedInfoModel("qwe");

            var response = await SteepshotApi[apiName].GetCommentsAsync(request, CancellationToken.None);

            Assert.That(response.Exception.Message.Contains("Wrong identifier."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Comments_Invalid_Url_But_Valid_User(KnownChains apiName)
        {
            var request = new NamedInfoModel("@asduj/qweqweqweqw");

            var response = await SteepshotApi[apiName].GetCommentsAsync(request, CancellationToken.None);

            Assert.That(response.Exception.Message.Contains("Wrong identifier."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories(KnownChains apiName)
        {
            // Arrange
            var request = new OffsetLimitModel();

            // Act
            var response = await SteepshotApi[apiName].GetCategoriesAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetCategoriesAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetCategoriesAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].SearchCategoriesAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].SearchCategoriesAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].SearchCategoriesAsync(request, CancellationToken.None);

            // Assert
            Assert.IsTrue(response.Exception.Message.StartsWith(nameof(LocalizationKeys.QueryMinLength)));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_Empty_Query(KnownChains apiName)
        {
            var request = new SearchWithQueryModel(" ");

            var response = await SteepshotApi[apiName].SearchCategoriesAsync(request, CancellationToken.None);

            Assert.IsTrue(response.Exception.Message.StartsWith(nameof(LocalizationKeys.EmptyCategory)));
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
            var response = await SteepshotApi[apiName].SearchCategoriesAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].SearchCategoriesAsync(request, CancellationToken.None);

            // Assert
            Assert.That(response.Exception.Message.Contains("Category used for offset was not found"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Categories_Search_With_User(KnownChains apiName)
        {
            var request = new SearchWithQueryModel("lif");
            
            var response = await SteepshotApi[apiName].SearchCategoriesAsync(request, CancellationToken.None);
            
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
            var response = await SteepshotApi[apiName].GetUserProfileAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetUserProfileAsync(request, CancellationToken.None);

            // Assert
            Assert.That(response.Exception.Message.Equals("User not found"));
        }

        [Test]
        [TestCase(KnownChains.Steem, "thecryptofiend")]
        [TestCase(KnownChains.Golos, "phoenix")]
        public async Task UserProfile_With_User(KnownChains apiName, string user)
        {
            // Arrange
            var request = new UserProfileModel(user) { Login = user };

            // Act
            var response = await SteepshotApi[apiName].GetUserProfileAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetUserFriendsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetUserFriendsAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].GetUserFriendsAsync(request, CancellationToken.None);

            // Assert
            Assert.IsTrue(response.Exception.Message.Equals("Account does not exist"));
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
            var response = await SteepshotApi[apiName].GetUserFriendsAsync(request, CancellationToken.None);

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
            var user = Users[apiName];
            var offset = string.Empty;
            var someResponsesAreHasFollowTrue = false;
            for (var i = 0; i < 10; i++)
            {
                var request = new UserFriendsModel(user.Login, FriendsType.Followers) { Login = user.Login, Offset = offset };
                var response = await SteepshotApi[apiName].GetUserFriendsAsync(request, CancellationToken.None);

                AssertResult(response);
                Assert.IsTrue(response.Result.Results != null);
                someResponsesAreHasFollowTrue = response.Result.Results.Any(x => x.HasFollowed == true);
                if (someResponsesAreHasFollowTrue)
                    break;

                offset = response.Result.Results.Last().Author;
            }

            Assert.IsTrue(someResponsesAreHasFollowTrue);
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
            var response = await SteepshotApi[apiName].GetPostInfoAsync(request, CancellationToken.None);

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
            //Assert.That(response.Result.NetVotes, Is.Not.Null);
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
            var response = await SteepshotApi[apiName].GetPostInfoAsync(request, CancellationToken.None);

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
            //Assert.That(response.Result.NetVotes, Is.Not.Null);
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
            var request = new NamedInfoModel("spam/@joseph.kalu/qweqeqwqweqweqwe");

            var response = await SteepshotApi[apiName].GetPostInfoAsync(request, CancellationToken.None);

            Assert.That(response.Exception.Message.Contains("Wrong identifier."));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel("aar");

            // Act
            var response = await SteepshotApi[apiName].SearchUserAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].SearchUserAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].SearchUserAsync(request, CancellationToken.None);

            // Assert
            Assert.IsTrue(response.Exception.Message.Equals("Query should have at least 3 characters"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Search_Empty_Query(KnownChains apiName)
        {
            // Arrange
            var request = new SearchWithQueryModel(" ");

            // Act
            var response = await SteepshotApi[apiName].SearchUserAsync(request, CancellationToken.None);

            // Assert
            Assert.IsTrue(response.Exception.Message.Equals(nameof(LocalizationKeys.EmptyCategory)));
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
            var response = await SteepshotApi[apiName].SearchUserAsync(request, CancellationToken.None);

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
            var request = new SearchWithQueryModel("aar") { Offset = "qweqweqwe" };
            var response = await SteepshotApi[apiName].SearchUserAsync(request, CancellationToken.None);

            Assert.IsTrue(response.Exception.Message.Equals("Username used for offset was not found"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task User_Exists_Check_Valid_Username(KnownChains apiName)
        {
            // Arrange
            var request = new UserExistsModel("pmartynov");

            // Act
            var response = await SteepshotApi[apiName].UserExistsCheckAsync(request, CancellationToken.None);

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
            var response = await SteepshotApi[apiName].UserExistsCheckAsync(request, CancellationToken.None);

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
                var operationResult = SteepshotApi[KnownChains.Steem].SearchUserAsync(request, cts.Token).Result;
            });

            // Assert
            Assert.That(ex.InnerException.Message, Is.EqualTo("A task was canceled."));
        }
    }
}