using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public sealed class UserFriendPresenter : ListPresenter<UserFriend>, IDisposable
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

        public async Task<ErrorBase> TryLoadNextPostVoters(string url)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNextPostVoters, url);
        }

        private async Task<ErrorBase> LoadNextPostVoters(string url, CancellationToken ct)
        {
            if (!VotersType.HasValue)
                return null;

            var request = new VotersModel(url, VotersType.Value)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit,
                Login = AppSettings.User.Login
            };

            var response = await Api.GetPostVoters(request, ct);

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
                NotifySourceChanged(nameof(TryLoadNextPostVoters), true);
            }
            return response.Error;
        }


        public async Task<ErrorBase> TryLoadNextUserFriends(string username)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNextUserFriends, username);
        }

        private async Task<ErrorBase> LoadNextUserFriends(string username, CancellationToken ct)
        {
            if (!FollowType.HasValue)
                return null;

            var request = new UserFriendsModel(username, FollowType.Value)
            {
                Login = AppSettings.User.Login,
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetUserFriends(request, ct);

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
                NotifySourceChanged(nameof(TryLoadNextUserFriends), true);
            }

            return response.Error;
        }


        public async Task<ErrorBase> TryLoadNextSearchUser(string query)
        {
            return await RunAsSingleTask(LoadNextSearchUser, query);
        }

        private async Task<ErrorBase> LoadNextSearchUser(string query, CancellationToken ct)
        {
            var request = new SearchWithQueryModel(query)
            {
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                Login = AppSettings.User.Login
            };

            var response = await Api.SearchUser(request, ct);

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
                NotifySourceChanged(nameof(TryLoadNextSearchUser), true);
            }
            return response.Error;
        }

        public async Task<ErrorBase> TryFollow(UserFriend item)
        {
            item.FollowedChanging = true;
            NotifySourceChanged(nameof(TryFollow), true);
            var error = await TryRunTask(Follow, OnDisposeCts.Token, item);
            item.FollowedChanging = false;
            NotifySourceChanged(nameof(TryFollow), true);
            return error;
        }

        private async Task<ErrorBase> Follow(UserFriend item, CancellationToken ct)
        {
            var hasFollowed = item.HasFollowed;
            var request = new FollowModel(AppSettings.User.UserInfo, item.HasFollowed ? Models.Enums.FollowType.UnFollow : Models.Enums.FollowType.Follow, item.Author);
            var response = await Api.Follow(request, ct);

            if (response.IsSuccess)
                item.HasFollowed = !hasFollowed;

            CashPresenterManager.Update(item);

            return response.Error;
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
