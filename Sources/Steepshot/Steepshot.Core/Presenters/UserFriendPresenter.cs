using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class UserFriendPresenter : ListPresenter<UserFriend>, IDisposable
    {
        private const int ItemsLimit = 40;
        public FriendsType? FollowType { get; set; }
        public VotersType? VotersType { get; set; }

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
            return await RunAsSingleTaskAsync(LoadNextPostVotersAsync, url).ConfigureAwait(false);
        }

        private async Task<Exception> LoadNextPostVotersAsync(string url, CancellationToken ct)
        {
            if (!VotersType.HasValue)
                return null;

            var request = new VotersModel(url, VotersType.Value)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit,
                Login = AppSettings.User.Login
            };

            var response = await Api.GetPostVotersAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    lock (Items)
                    {
                        Items.AddRange(Items.Count == 0 ? voters : voters.Skip(1));
                        CashPresenterManager.Add(Items.Count == 0 ? voters : voters.Skip(1));
                    }

                    OffsetUrl = voters.Last().Author;
                }

                if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNextPostVotersAsync), true);
            }
            return response.Exception;
        }


        public async Task<Exception> TryLoadNextUserFriendsAsync(string username)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTaskAsync(LoadNextUserFriendsAsync, username).ConfigureAwait(false);
        }

        private async Task<Exception> LoadNextUserFriendsAsync(string username, CancellationToken ct)
        {
            if (!FollowType.HasValue)
                return null;

            var request = new UserFriendsModel(username, FollowType.Value)
            {
                Login = AppSettings.User.Login,
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetUserFriendsAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                var result = response.Result.Results;
                if (result.Count > 0)
                {
                    lock (Items)
                    {
                        Items.AddRange(Items.Count == 0 ? result : result.Skip(1));
                        CashPresenterManager.Add(Items.Count == 0 ? result : result.Skip(1));
                    }

                    OffsetUrl = result.Last().Author;
                }

                if (result.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNextUserFriendsAsync), true);
            }

            return response.Exception;
        }


        public async Task<Exception> TryLoadNextSearchUserAsync(string query)
        {
            return await RunAsSingleTaskAsync(LoadNextSearchUserAsync, query).ConfigureAwait(false);
        }

        private async Task<Exception> LoadNextSearchUserAsync(string query, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(query) || query.Length <= 2)
                return new OperationCanceledException();

            var request = new SearchWithQueryModel(query)
            {
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                Login = AppSettings.User.Login
            };

            var response = await Api.SearchUserAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                var result = response.Result.Results;
                if (result.Count > 0)
                {
                    lock (Items)
                    {
                        Items.AddRange(Items.Count == 0 ? result : result.Skip(1));
                        CashPresenterManager.Add(Items.Count == 0 ? result : result.Skip(1));
                    }

                    OffsetUrl = result.Last().Author;
                }

                if (result.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNextSearchUserAsync), true);
            }
            return response.Exception;
        }

        public async Task<Exception> TryFollowAsync(UserFriend item)
        {
            item.FollowedChanging = true;
            NotifySourceChanged(nameof(TryFollowAsync), true);
            var exception = await TryRunTaskAsync(FollowAsync, OnDisposeCts.Token, item).ConfigureAwait(false);
            item.FollowedChanging = false;
            NotifySourceChanged(nameof(TryFollowAsync), true);
            return exception;
        }

        private async Task<Exception> FollowAsync(UserFriend item, CancellationToken ct)
        {
            var hasFollowed = item.HasFollowed;
            var request = new FollowModel(AppSettings.User.UserInfo, item.HasFollowed ? Models.Enums.FollowType.UnFollow : Models.Enums.FollowType.Follow, item.Author);
            var response = await Api.FollowAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccess)
                item.HasFollowed = !hasFollowed;

            CashPresenterManager.Update(item);

            return response.Exception;
        }

        public override void Clear(bool isNotify = true)
        {
            lock (Items)
            {
                CashPresenterManager.RemoveAll(Items);
            }
            base.Clear(isNotify);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (Items)
                    {
                        CashPresenterManager.RemoveAll(Items);
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
