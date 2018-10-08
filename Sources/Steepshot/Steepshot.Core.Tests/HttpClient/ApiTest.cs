﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using System.Threading.Tasks;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Tests.HttpClient
{
    [TestFixture]
    public class ApiTest : BaseTests
    {
        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task LoginWithPostingKeyTest(KnownChains apiName)
        {
            var user = Users[apiName].UserInfo;
            var request = new ValidatePrivateKeyModel(user.Login, user.PostingKey, KeyRoleType.Posting);
            var response = await Api[apiName].ValidatePrivateKeyAsync(request, CancellationToken.None);
            AssertResult(response);
            Assert.That(response.IsSuccess, Is.True);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task UploadMediaTest(KnownChains apiName)
        {
            var user = Users[apiName].UserInfo;

            // 1) Create new post
            var path = GetTestImagePath();
            var stream = new FileStream(GetTestImagePath(), FileMode.Open);
            var uploadImageModel = new UploadMediaModel(user, stream, Path.GetExtension(path));
            var servResp = await SteepshotClient.UploadMediaAsync(uploadImageModel, CancellationToken.None);
            AssertResult(servResp);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        public async Task PreparePostTest(KnownChains apiName)
        {
            var user = Users[apiName].UserInfo;
            var model = new PreparePostModel(user, AppInfo.GetModel())
            {
                Title = "Test",
                Description = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Media = new[]
                {
                    new MediaModel
                    {
                        Url = "http://steepshot.org/api/v1/image/034e7cc2-90df-4186-b475-9b7d4166e0a4.jpeg",
                        IpfsHash = "QmUHaQDMc46pR21fNFt1Gxo5YeeFxD4uENywbevXe5XXWM",
                        Size = new FrameSize
                        {
                            Height = 194,
                            Width = 194
                        }
                    }
                },
                Tags = new[] { "test" }
            };

            var createPostResponse = await SteepshotApi[apiName].PreparePostAsync(model, CancellationToken.None);
            AssertResult(createPostResponse);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task CreateCommentTest(KnownChains apiName)
        {
            var user = Users[apiName].UserInfo;

            // Load last created post
            var userPostsRequest = new UserPostsModel(user.Login);
            userPostsRequest.ShowNsfw = true;
            userPostsRequest.ShowLowRated = true;
            var userPostsResponse = await SteepshotApi[apiName].GetUserPostsAsync(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First();

            // 2) Create new comment
            // Wait for 20 seconds before commenting
            Thread.Sleep(TimeSpan.FromSeconds(20));
            var createCommentModel = new CreateOrEditCommentModel(user, lastPost, $"Test comment {DateTime.Now:G}", AppInfo);
            var createCommentResponse = await CreateOrEditCommentAsync(apiName, createCommentModel, CancellationToken.None);
            AssertResult(createCommentResponse);
            Assert.That(createCommentResponse.IsSuccess, Is.True);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));

            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoModel(lastPost.Url);
            var commentsResponse = await SteepshotApi[apiName].GetCommentsAsync(getCommentsRequest, CancellationToken.None);
            AssertResult(commentsResponse);

            Assert.IsNotNull(commentsResponse.Result.Results.FirstOrDefault(i => i.Url.EndsWith(createCommentModel.Permlink)));
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task VotePostTest(KnownChains apiName)
        {
            var user = Users[apiName].UserInfo;

            // Load last created post
            var userPostsRequest = new PostsModel(PostType.New) { Login = user.Login };
            var userPostsResponse = await SteepshotApi[apiName].GetPostsAsync(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => !i.Vote);

            // 4) Vote up
            var voteUpRequest = new VoteModel(user, lastPost, VoteType.Up) { VoteDelay = 0 };
            var voteUpResponse = await Api[apiName].VoteAsync(voteUpRequest, CancellationToken.None);
            AssertResult(voteUpResponse);
            Assert.That(voteUpResponse.IsSuccess, Is.True);
            //Assert.IsTrue(lastPost.TotalPayoutReward <= voteUpResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            userPostsRequest.Offset = lastPost.Url;
            var userPostsResponse2 = await SteepshotApi[apiName].GetPostsAsync(userPostsRequest, CancellationToken.None);
            // Check if last post was voted
            AssertResult(userPostsResponse2);
            var post = userPostsResponse2.Result.Results.FirstOrDefault(i => i.Url.EndsWith(lastPost.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(post);
            Console.WriteLine("The server still updates the history");
            //Assert.That(post.Vote, Is.True);

            // 3) Vote down
            var voteDownRequest = new VoteModel(user, lastPost, VoteType.Down) { VoteDelay = 0 };
            var voteDownResponse = await Api[apiName].VoteAsync(voteDownRequest, CancellationToken.None);
            AssertResult(voteDownResponse);
            Assert.That(voteDownResponse.IsSuccess, Is.True);
            //Assert.IsTrue(lastPost.TotalPayoutReward >= voteDownResponse.Result.NewTotalPayoutReward);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            var userPostsResponse3 = await SteepshotApi[apiName].GetPostsAsync(userPostsRequest, CancellationToken.None);
            // Check if last post was voted
            AssertResult(userPostsResponse3);
            post = userPostsResponse3.Result.Results.FirstOrDefault(i => i.Url.Equals(lastPost.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(post);
            Console.WriteLine("The server still updates the history");
            //Assert.That(post.Vote, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem)]
        [TestCase(KnownChains.Golos)]
        [Ignore("For hand test only")]
        public async Task VoteCommentTest(KnownChains apiName)
        {
            var user = Users[apiName].UserInfo;

            // Load last created post
            var userPostsRequest = new UserPostsModel(user.Login) { ShowLowRated = true, ShowNsfw = true };
            var userPostsResponse = await SteepshotApi[apiName].GetUserPostsAsync(userPostsRequest, CancellationToken.None);
            AssertResult(userPostsResponse);
            var lastPost = userPostsResponse.Result.Results.First(i => i.Children > 0);
            // Load comments for this post and check them
            var getCommentsRequest = new NamedInfoModel(lastPost.Url);
            var commentsResponse = await SteepshotApi[apiName].GetCommentsAsync(getCommentsRequest, CancellationToken.None);

            // 5) Vote up comment
            var post = commentsResponse.Result.Results.First();
            var voteUpCommentRequest = new VoteModel(user, post, VoteType.Up) { VoteDelay = 0 };
            var voteUpCommentResponse = await Api[apiName].VoteAsync(voteUpCommentRequest, CancellationToken.None);
            AssertResult(voteUpCommentResponse);
            Assert.That(voteUpCommentResponse.IsSuccess, Is.True);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            getCommentsRequest.Login = user.Login;
            var commentsResponse2 = await SteepshotApi[apiName].GetCommentsAsync(getCommentsRequest, CancellationToken.None);
            // Check if last comment was voted
            AssertResult(commentsResponse2);
            var comm = commentsResponse2.Result.Results.FirstOrDefault(i => i.Url.Equals(post.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(comm);
            Assert.That(comm.Vote, Is.True);

            // 6) Vote down comment
            var voteDownCommentRequest = new VoteModel(user, post, VoteType.Down) { VoteDelay = 0 };
            var voteDownCommentResponse = await Api[apiName].VoteAsync(voteDownCommentRequest, CancellationToken.None);
            AssertResult(voteDownCommentResponse);
            Assert.That(voteDownCommentResponse.IsSuccess, Is.True);

            // Wait for data to be writed into blockchain
            Thread.Sleep(TimeSpan.FromSeconds(15));
            getCommentsRequest.Login = user.Login;
            var commentsResponse3 = await SteepshotApi[apiName].GetCommentsAsync(getCommentsRequest, CancellationToken.None);
            // Check if last comment was voted
            AssertResult(commentsResponse3);
            comm = commentsResponse3.Result.Results.FirstOrDefault(i => i.Url.Equals(post.Url, StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(comm);
            Assert.That(comm.Vote, Is.False);
        }

        [Test]
        [TestCase(KnownChains.Steem, "asduj")]
        [TestCase(KnownChains.Golos, "pmartynov")]
        [Ignore("For hand test only")]
        public async Task FollowTest(KnownChains apiName, string followUser)
        {
            var user = Users[apiName].UserInfo;

            // 7) Follow
            var followRequest = new FollowModel(user, FollowType.Follow, followUser);
            var followResponse = await Api[apiName].FollowAsync(followRequest, CancellationToken.None);
            AssertResult(followResponse);
            Assert.IsTrue(followResponse.IsSuccess);

            // 8) UnFollow
            var unfollowRequest = new FollowModel(user, FollowType.UnFollow, followUser);
            var unfollowResponse = await Api[apiName].FollowAsync(unfollowRequest, CancellationToken.None);
            AssertResult(unfollowResponse);
            Assert.IsTrue(unfollowResponse.IsSuccess);
        }


        [Test]
        [TestCase(KnownChains.Steem, "joseph.kalu")]
        [TestCase(KnownChains.Golos, "joseph.kalu")]
        [Ignore("For hand test only")]
        public async Task UpdateUserProfileTest(KnownChains apiName, string followUser)
        {
            var user = Users[apiName].UserInfo;

            var userProfileModel = new UserProfileModel(user.Login);
            var profileResponse = await SteepshotApi[apiName].GetUserProfileAsync(userProfileModel, CancellationToken.None);
            AssertResult(profileResponse);
            Assert.IsTrue(profileResponse.IsSuccess);
            var profile = profileResponse.Result;

            var updateUserProfileModel = new UpdateUserProfileModel(user.Login, user.PostingKey)
            {
                About = profile.About,
                Location = profile.Location,
                Name = profile.Name,
                ProfileImage = profile.ProfileImage,
                Website = profile.Website
            };
            var response = await Api[apiName].UpdateUserProfileAsync(updateUserProfileModel, CancellationToken.None);
            AssertResult(response);
            Assert.IsTrue(response.IsSuccess);
        }
    }
}
