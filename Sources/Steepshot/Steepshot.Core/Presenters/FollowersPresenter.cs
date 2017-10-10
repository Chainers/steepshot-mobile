using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class FollowersPresenter : ListPresenter<UserFriend>
    {
        private const int ItemsLimit = 40;
        private readonly FriendsType? _followType;

        public FriendsType? FollowType => _followType;

        public FollowersPresenter()
        {
        }

        public FollowersPresenter(FriendsType followType)
        {
            _followType = followType;
        }

        public UserFriend FirstOrDefault(Func<UserFriend, bool> func)
        {
            lock (Items)
                return Items.FirstOrDefault(func);
        }

        public async Task<List<string>> TryLoadNextUserFriends(string username)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNextUserFriends, username);
        }

        private async Task<List<string>> LoadNextUserFriends(CancellationTokenSource cts, string username)
        {
            if (_followType == null)
                return null;

            var request = new UserFriendsRequest(username, _followType.Value)
            {
                Login = User.Login,
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetUserFriends(request, cts);
            if (response.Success && response.Result?.Results != null)
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



        public async Task<List<string>> TryLoadNextSearchUser(CancellationTokenSource cts, string query)
        {
            return await LoadNextSearchUser(cts, query);
        }


        private async Task<List<string>> LoadNextSearchUser(CancellationTokenSource cts, string query)
        {
            var request = new SearchWithQueryRequest(query)
            {
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                Login = User.Login
            };

            var response = await Api.SearchUser(request, cts);
            if (response.Success && response.Result?.Results != null)
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
            return await TryRunTask(Follow, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), item);
        }

        private async Task<List<string>> Follow(CancellationTokenSource cts, UserFriend item)
        {
            var request = new FollowRequest(User.UserInfo, item.HasFollowed ? Models.Requests.FollowType.UnFollow : Models.Requests.FollowType.Follow, item.Author);
            var response = await Api.Follow(request, cts);
            if (response.Success)
            {
                item.HasFollowed = !item.HasFollowed;
            }
            return response.Errors;
        }
    }
}
