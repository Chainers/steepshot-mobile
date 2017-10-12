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
    public sealed class UserProfilePresenter : BasePostPresenter
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
                Clear();

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
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? voters : voters.Skip(1));

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

        public async Task<OperationResult<FollowResponse>> TryFollow(bool hasFollowed)
        {
            return await TryRunTask(Follow, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), hasFollowed ? FollowType.UnFollow : FollowType.Follow);
        }

        private async Task<OperationResult<FollowResponse>> Follow(CancellationTokenSource cts, FollowType followType)
        {
            var request = new FollowRequest(User.UserInfo, followType, _username);
            return await Api.Follow(request, cts);
        }
    }
}
