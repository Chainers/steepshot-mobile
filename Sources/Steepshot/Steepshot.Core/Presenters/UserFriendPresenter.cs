using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class UserFriendPresenter : ListPresenter<UserFriend>
    {
        private const int ItemsLimit = 40;
        private readonly FriendsType? _followType;

        public FriendsType? FollowType => _followType;

        public UserFriendPresenter()
        {
        }

        public UserFriendPresenter(FriendsType followType)
        {
            _followType = followType;
        }

        public UserFriend FirstOrDefault(Func<UserFriend, bool> func)
        {
            lock (Items)
                return Items.FirstOrDefault(func);
        }


        public async Task<List<string>> TryLoadNextPostVoters(string url)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNextPostVoters, url);
        }

        private async Task<List<string>> LoadNextPostVoters(CancellationToken ct, string url)
        {
            var request = new InfoRequest(url)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit,
                Login = User.Login
            };

            var response = await Api.GetPostVoters(request, ct);
            if (response == null)
                return null;

            if (response.Success)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? voters : voters.Skip(1));

                    OffsetUrl = voters.Last().Author;
                }

                if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }


        public async Task<List<string>> TryLoadNextUserFriends(string username)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNextUserFriends, username);
        }

        private async Task<List<string>> LoadNextUserFriends(CancellationToken ct, string username)
        {
            if (_followType == null)
                return null;

            var request = new UserFriendsRequest(username, _followType.Value)
            {
                Login = User.Login,
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetUserFriends(request, ct);
            if (response == null)
                return null;

            if (response.Success)
            {
                var result = response.Result.Results;
                if (result.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? result : result.Skip(1));

                    OffsetUrl = result.Last().Author;
                }

                if (result.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }

            return response.Errors;
        }


        public async Task<List<string>> TryLoadNextSearchUser(CancellationToken ct, string query)
        {
            return await LoadNextSearchUser(ct, query);
        }

        private async Task<List<string>> LoadNextSearchUser(CancellationToken ct, string query)
        {
            var request = new SearchWithQueryRequest(query)
            {
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                Login = User.Login
            };

            var response = await Api.SearchUser(request, ct);
            if (response == null)
                return null;

            if (response.Success)
            {
                var result = response.Result.Results;
                if (result.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? result : result.Skip(1));

                    OffsetUrl = result.Last().Author;
                }

                if (result.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }


        public async Task<List<string>> TryFollow(UserFriend item)
        {
            return await TryRunTask(Follow, OnDisposeCts.Token, item);
        }

        private async Task<List<string>> Follow(CancellationToken ct, UserFriend item)
        {
            var request = new FollowRequest(User.UserInfo, (bool)item.HasFollowed ? Models.Requests.FollowType.UnFollow : Models.Requests.FollowType.Follow, item.Author);
            item.HasFollowed = null;
            var response = await Api.Follow(request, ct);
            if (response == null)
                return null;

            if (response.Success)
                item.HasFollowed = request.Type == Models.Requests.FollowType.Follow;
            else
                item.HasFollowed = request.Type != Models.Requests.FollowType.Follow;
            return response.Errors;
        }
    }
}
