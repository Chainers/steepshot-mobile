using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public sealed class UserProfilePresenter : BaseFeedPresenter
    {
        private readonly string _username;
        private const int ItemsLimit = 20;

        public UserProfilePresenter(string username)
        {
            _username = username;
        }

        public async Task<List<string>> TryLoadNextPosts(bool needRefresh = false)
        {
            if (needRefresh)
                ClearPosts();

            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextPosts);
        }

        private async Task<List<string>> LoadNextPosts(CancellationTokenSource cts)
        {
            var req = new UserPostsRequest(_username)
            {
                Login = User.Login,
                Offset = OffsetUrl,
                Limit = ItemsLimit,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            var response = await Api.GetUserPosts(req, cts);

            if (response.Success)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    lock (Posts)
                        Posts.AddRange(string.IsNullOrEmpty(OffsetUrl) ? voters : voters.Skip(1));

                    OffsetUrl = voters.Last().Url;
                }
                if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }


        public Task<OperationResult<UserProfileResponse>> TryGetUserInfo(string user)
        {
            return TryRunTask(GetUserInfo, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), user);
        }

        private Task<OperationResult<UserProfileResponse>> GetUserInfo(CancellationTokenSource cts, string user)
        {
            var req = new UserProfileRequest(user)
            {
                Login = User.Login
            };
            return Api.GetUserProfile(req, cts);
        }


        public Task<OperationResult<FollowResponse>> TryFollow(int hasFollowed)
        {
            return TryRunTask(Follow, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), hasFollowed);
        }

        private Task<OperationResult<FollowResponse>> Follow(CancellationTokenSource cts, int hasFollowed)
        {
            var request = new FollowRequest(User.UserInfo, hasFollowed == 0 ? FollowType.Follow : FollowType.UnFollow, _username);
            return Api.Follow(request, cts);
        }
    }
}
