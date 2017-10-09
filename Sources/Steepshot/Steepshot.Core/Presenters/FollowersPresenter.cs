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
    public sealed class FollowersPresenter : ListPresenter
    {
        private const int ItemsLimit = 40;
        private readonly List<UserFriend> _users;
        private readonly FriendsType? _followType;

        public FriendsType? FollowType => _followType;
        public override int Count => _users.Count;

        public UserFriend this[int position]
        {
            get
            {
                lock (_users)
                {
                    if (position > -1 && position < _users.Count)
                        return _users[position];
                }
                return null;
            }
        }

        public FollowersPresenter()
        {
            _users = new List<UserFriend>();
        }

        public FollowersPresenter(FriendsType followType) : this()
        {
            _followType = followType;
        }

        public void Clear()
        {
            lock (_users)
                _users.Clear();
            OffsetUrl = string.Empty;
            IsLastReaded = false;
        }

        public UserFriend FirstOrDefault(Func<UserFriend, bool> func)
        {
            lock (_users)
                return _users.FirstOrDefault(func);
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
                    lock (_users)
                        _users.AddRange(string.IsNullOrEmpty(OffsetUrl) ? result : result.Skip(1));

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
                    lock (_users)
                        _users.AddRange(string.IsNullOrEmpty(OffsetUrl) ? result : result.Skip(1));

                    OffsetUrl = result.Last().Author;
                }

                if (result.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }


        public async Task<OperationResult<FollowResponse>> TryFollow(UserFriend item)
        {
            return await TryRunTask(Follow, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), item);
        }

        private async Task<OperationResult<FollowResponse>> Follow(CancellationTokenSource cts, UserFriend item)
        {
            var request = new FollowRequest(User.UserInfo, item.HasFollowed ? Models.Requests.FollowType.UnFollow : Models.Requests.FollowType.Follow, item.Author);
            return await Api.Follow(request, cts);
        }

        public void InverseFollow(int position)
        {
            lock (_users)
            {
                if (position > -1 && position < _users.Count)
                {
                    var user = _users[position];
                    user.HasFollowed = !user.HasFollowed;
                }
            }
        }
    }
}
