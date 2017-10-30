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
        private const int ItemsLimit = 18;

        public string UserName { get; set; }
        

        public async Task<List<string>> TryLoadNextPosts(bool needRefresh = false)
        {
            if (needRefresh)
                Clear();

            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextPosts);
        }

        private async Task<List<string>> LoadNextPosts(CancellationToken ct)
        {
            var req = new UserPostsRequest(UserName)
            {
                Login = User.Login,
                Offset = OffsetUrl,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            var response = await Api.GetUserPosts(req, ct);
            if (response == null)
                return null;

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
            return TryRunTask<string, UserProfileResponse>(GetUserInfo, OnDisposeCts.Token, user);
        }

        private Task<OperationResult<UserProfileResponse>> GetUserInfo(CancellationToken ct, string user)
        {
            var req = new UserProfileRequest(user)
            {
                Login = User.Login,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            return Api.GetUserProfile(req, ct);
        }

        public async Task<OperationResult<FollowResponse>> TryFollow(bool hasFollowed)
        {
            return await TryRunTask<FollowType, FollowResponse>(Follow, OnDisposeCts.Token, hasFollowed ? FollowType.UnFollow : FollowType.Follow);
        }

        private async Task<OperationResult<FollowResponse>> Follow(CancellationToken ct, FollowType followType)
        {
            var request = new FollowRequest(User.UserInfo, followType, UserName);
            return await Api.Follow(request, ct);
        }
    }
}
