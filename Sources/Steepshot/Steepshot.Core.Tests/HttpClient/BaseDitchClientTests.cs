using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests.HttpClient
{
    [TestFixture]
    public class BaseDitchClientTests : BaseTests
    {
        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Login_With_Posting_Key_Invalid_Credentials(KnownChains apiName)
        {
            var user = Users[apiName];
            user.Login += "x";
            user.PostingKey += "x";
            var request = new ValidatePrivateKeyModel(user.Login, user.PostingKey, KeyRoleType.Posting);

            var response = await Api[apiName].ValidatePrivateKeyAsync(request, CancellationToken.None);

            Assert.IsTrue(response.Exception.Message.StartsWith(nameof(LocalizationKeys.WrongPrivatePostingKey)));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Login_With_Posting_Key_Wrong_PostingKey(KnownChains apiName)
        {
            var user = Users[apiName];
            user.PostingKey += "x";
            var request = new ValidatePrivateKeyModel(user.Login, user.PostingKey, KeyRoleType.Posting);

            var response = await Api[apiName].ValidatePrivateKeyAsync(request, CancellationToken.None);

            Assert.IsTrue(response.Exception.Message.StartsWith(nameof(LocalizationKeys.WrongPrivatePostingKey)));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Login_With_Posting_Key_Wrong_Username(KnownChains apiName)
        {
            var user = Users[apiName];
            user.Login += "x";
            var request = new ValidatePrivateKeyModel(user.Login, user.PostingKey, KeyRoleType.Posting);

            var response = await Api[apiName].ValidatePrivateKeyAsync(request, CancellationToken.None);

            Assert.IsTrue(response.Exception.Message.StartsWith("13 N5boost16exception_detail10clone_implINS0_19error_info_injectorISt12out_of_rangeEEEE: unknown key"));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task Vote_Up_Already_Voted(KnownChains apiName)
        {
            var user = Users[apiName];
            var userPostsRequest = new CensoredNamedRequestWithOffsetLimitModel();
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.Login = user.Login;
            var posts = await Api[apiName].GetUserRecentPostsAsync(userPostsRequest, CancellationToken.None);
            Assert.IsTrue(posts.IsSuccess);
            var postForVote = posts.Result.Results.FirstOrDefault(i => i.Vote == false);
            Assert.IsNotNull(postForVote);

            var request = new VoteModel(Users[apiName], postForVote, VoteType.Up);
            var response = await Api[apiName].VoteAsync(request, CancellationToken.None);
            AssertResult(response);
            Thread.Sleep(2000);

            var response2 = await Api[apiName].VoteAsync(request, CancellationToken.None);
            AssertResult(response2);

            Assert.That(response2.Exception.Message.Contains("You have already voted in a similar way.")
                        || response2.Exception.Message.Contains("You`ve already liked this post a few times. Please try another one.")
                        || response2.Exception.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Exception.Message.Contains("Cannot vote again on a comment after payout.")
                        || response2.Exception.Message.Contains("Duplicate transaction check failed")
                        || response2.Exception.Message.Contains("Vote weight cannot be 0.")
                        || response2.Exception.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Exception.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task Vote_Down_Already_Voted(KnownChains apiName)
        {
            // Load last post
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var posts = await Api[apiName].GetUserPostsAsync(userPostsRequest, CancellationToken.None);
            var lastPost = posts.Result.Results.First();

            // Arrange
            var request = new VoteModel(Users[apiName], lastPost, VoteType.Down);

            // Act
            var response = await Api[apiName].VoteAsync(request, CancellationToken.None);
            Thread.Sleep(2000);
            var response2 = await Api[apiName].VoteAsync(request, CancellationToken.None);

            // Assert
            AssertResult(response2);
            Assert.That(response2.Exception.Message.Contains("You have already voted in a similar way.")
                        || response2.Exception.Message.Contains("You`ve already liked this post a few times. Please try another one.")
                        || response2.Exception.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Exception.Message.Contains("Duplicate transaction check failed")
                        || response2.Exception.Message.Contains("Vote weight cannot be 0.")
                        || response2.Exception.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Exception.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task Flag_Up_Already_Flagged(KnownChains apiName)
        {
            // Load last post
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var posts = await Api[apiName].GetUserPostsAsync(userPostsRequest, CancellationToken.None);
            var lastPost = posts.Result.Results.First();

            // Arrange
            var request = new VoteModel(Users[apiName], lastPost, VoteType.Flag);

            // Act
            var response = await Api[apiName].VoteAsync(request, CancellationToken.None);
            var response2 = await Api[apiName].VoteAsync(request, CancellationToken.None);

            // Assert
            AssertResult(response2);
            Assert.That(response2.Exception.Message.Contains("You have already voted in a similar way.")
                        || response2.Exception.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Exception.Message.Contains("Duplicate transaction check failed")
                        || response2.Exception.Message.Contains("Vote weight cannot be 0.")
                        || response2.Exception.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Exception.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task Flag_Down_Already_Flagged(KnownChains apiName)
        {
            // Load last post
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var posts = await Api[apiName].GetUserPostsAsync(userPostsRequest, CancellationToken.None);
            var lastPost = posts.Result.Results.First();

            // Arrange
            var request = new VoteModel(Users[apiName], lastPost, VoteType.Down);

            // Act
            var response = await Api[apiName].VoteAsync(request, CancellationToken.None);
            var response2 = await Api[apiName].VoteAsync(request, CancellationToken.None);

            // Assert
            AssertResult(response2);
            AssertResult(response2);
            Assert.That(response2.Exception.Message.Contains("You have already voted in a similar way.")
                        || response2.Exception.Message.Contains("Can only vote once every 3 seconds.")
                        || response2.Exception.Message.Contains("Duplicate transaction check failed")
                        || response2.Exception.Message.Contains("Vote weight cannot be 0.")
                        || response2.Exception.Message.Contains("('Voter has used the maximum number of vote changes on this comment.',)"), response2.Exception.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task CreateComment_20_Seconds_Delay(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            var userPostsResponse = await Api[apiName].GetUserPostsAsync(userPostsRequest, CancellationToken.None);
            var lastPost = userPostsResponse.Result.Results.First();
            var body = $"Test comment {DateTime.Now:G}";
            var createCommentModel = new CreateOrEditCommentModel(Users[apiName], lastPost, body, AppSettings.AppInfo);

            // Act
            var response1 = await Api[apiName].CreateOrEditCommentAsync(createCommentModel, CancellationToken.None);
            var response2 = await Api[apiName].CreateOrEditCommentAsync(createCommentModel, CancellationToken.None);

            // Assert
            AssertResult(response1);
            AssertResult(response2);
            Assert.That(response2.Exception.Message.Contains("You may only comment once every 20 seconds.") || response2.Exception.Message.Contains("Duplicate transaction check failed"), response2.Exception.Message);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task EditCommentTest(KnownChains apiName)
        {
            // Arrange
            var user = Users[apiName];
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowLowRated = true;
            userPostsRequest.ShowNsfw = true;
            var userPostsResponse = await Api[apiName].GetUserPostsAsync(userPostsRequest, CancellationToken.None);

            var post = userPostsResponse.Result.Results.FirstOrDefault(i => i.Children > 0);
            Assert.IsNotNull(post);
            var namedRequest = new NamedInfoModel(post.Url);
            var comments = await Api[apiName].GetCommentsAsync(namedRequest, CancellationToken.None);
            var comment = comments.Result.Results.FirstOrDefault(i => i.Author.Equals(user.Login));
            Assert.IsNotNull(comment);

            var editCommentRequest = new CreateOrEditCommentModel(user, post, comment, comment.Body += $" edited {DateTime.Now}", AppSettings.AppInfo);

            var result = await Api[apiName].CreateOrEditCommentAsync(editCommentRequest, CancellationToken.None);
            AssertResult(result);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task Upload_Empty_Photo(KnownChains apiName)
        {
            var request = new UploadMediaModel(Users[apiName], new MemoryStream(), ".jpg");
            var response = await Api[apiName].UploadMediaAsync(request, CancellationToken.None);
            Assert.IsTrue(response.Exception.Message.StartsWith("The submitted file is empty."));
        }
    }
}