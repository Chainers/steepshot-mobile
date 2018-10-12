using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class UserFriendPresenter : ListPresenter<UserFriend>, IDisposable
    {
        private const int ItemsLimit = 40;
        private readonly SteepshotApiClient _steepshotApiClient;
        private readonly BaseDitchClient _ditchClient;
        private readonly User _user;

        public FriendsType? FollowType { get; set; }
        public VotersType? VotersType { get; set; }


        public UserFriendPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, User user)
            : base(connectionService, logService)
        {
            _steepshotApiClient = steepshotApiClient;
            _user = user;
            _ditchClient = ditchClient;
        }

        public UserFriend FirstOrDefault(Func<UserFriend, bool> func)
        {
            lock (Items)
                return Items.FirstOrDefault(func);
        }

        public List<UserFriend> FindAll(Predicate<UserFriend> match)
        {
            lock (Items)
                return Items.FindAll(match);
        }

        public async Task<Exception> TryLoadNextPostVotersAsync(string url)
        {
            if (IsLastReaded)
                return null;

            if (!VotersType.HasValue)
                return null;

            var request = new VotersModel(url, VotersType.Value)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit,
                Login = _user.Login
            };

            var response = await RunAsSingleTaskAsync(_steepshotApiClient.GetPostVotersAsync, request)
                .ConfigureAwait(true);

            if (response.IsSuccess)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    lock (Items)
                    {
                        Items.AddRange(Items.Count == 0 ? voters : voters.Skip(1));
                        CashManager.Add(Items.Count == 0 ? voters : voters.Skip(1));
                    }

                    OffsetUrl = voters.Last().Author;
                }

                if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNextPostVotersAsync), true);
            }
            return response.Exception;
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> TryLoadNextUserFriendsAsync(string username)
        {
            if (IsLastReaded)
                return new OperationResult<ListResponse<UserFriend>>(new OperationCanceledException());

            if (!FollowType.HasValue)
                return new OperationResult<ListResponse<UserFriend>>(new OperationCanceledException());

            var request = new UserFriendsModel(username, FollowType.Value)
            {
                Login = _user.Login,
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await RunAsSingleTaskAsync(_steepshotApiClient.GetUserFriendsAsync, request)
                .ConfigureAwait(true);

            if (response.IsSuccess)
            {
                var result = response.Result.Results;
                if (result.Count > 0)
                {
                    lock (Items)
                    {
                        Items.AddRange(Items.Count == 0 ? result : result.Skip(1));
                        CashManager.Add(Items.Count == 0 ? result : result.Skip(1));
                    }

                    OffsetUrl = result.Last().Author;
                }

                if (result.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNextUserFriendsAsync), true);
            }

            return response;
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> TryLoadNextSearchUserAsync(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length <= 2)
                return new OperationResult<ListResponse<UserFriend>>(new OperationCanceledException());

            var request = new SearchWithQueryModel(query)
            {
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                Login = _user.Login
            };

            var response = await RunAsSingleTaskAsync(_steepshotApiClient.SearchUserAsync, request)
                .ConfigureAwait(true);

            if (response.IsSuccess)
            {
                var result = response.Result.Results;
                if (result.Count > 0)
                {
                    lock (Items)
                    {
                        Items.AddRange(Items.Count == 0 ? result : result.Skip(1));
                        CashManager.Add(Items.Count == 0 ? result : result.Skip(1));
                    }

                    OffsetUrl = result.Last().Author;
                }

                if (result.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNextSearchUserAsync), true);
            }
            return response;
        }

        public async Task<OperationResult<VoidResponse>> TryFollowAsync(UserFriend userFriend)
        {
            userFriend.FollowedChanging = true;
            NotifySourceChanged(nameof(TryFollowAsync), true);

            var hasFollowed = userFriend.HasFollowed;
            var request = new FollowModel(_user.UserInfo, hasFollowed ? Models.Enums.FollowType.UnFollow : Models.Enums.FollowType.Follow, userFriend.Author);
            var result = await TaskHelper.TryRunTaskAsync(_ditchClient.FollowAsync, request, OnDisposeCts.Token).ConfigureAwait(true);

            if (result.IsSuccess)
                userFriend.HasFollowed = !hasFollowed;

            CashManager.Update(userFriend);

            userFriend.FollowedChanging = false;
            NotifySourceChanged(nameof(TryFollowAsync), true);
            return result;
        }

        public override void Clear(bool isNotify)
        {
            lock (Items)
                CashManager.RemoveAll(Items);

            base.Clear(isNotify);
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (Items)
                    {
                        CashManager.RemoveAll(Items);
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BasePostPresenter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
