using System;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Presenters
{
    public sealed class UserProfilePresenter : BasePostPresenter
    {
        private const int ItemsLimit = 18;

        public string UserName { get; set; }

        public UserProfileResponse UserProfileResponse { get; private set; }

        public Action SubscriptionsUpdated;


        public UserProfilePresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, User user, SteepshotClient steepshotClient)
            : base(connectionService, logService, ditchClient, steepshotApiClient, user, steepshotClient)
        {
        }


        public async Task<Exception> TryLoadNextPostsAsync()
        {
            if (IsLastReaded)
                return null;

            var request = new UserPostsModel(UserName)
            {
                Login = User.Login,
                Offset = OffsetUrl,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await RunAsSingleTaskAsync(SteepshotApiClient.GetUserPostsAsync, request)
                    .ConfigureAwait(true);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextPostsAsync));
            } while (isNeedRepeat);

            return exception;
        }

        public async Task<OperationResult<UserProfileResponse>> TryGetUserInfoAsync(string user)
        {
            var req = new UserProfileModel(user)
            {
                Login = User.Login,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            var result = await TaskHelper
                .TryRunTaskAsync(SteepshotApiClient.GetUserProfileAsync, req, OnDisposeCts.Token)
                .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                UserProfileResponse = result.Result;
                CashManager.Add(UserProfileResponse);
                NotifySourceChanged(nameof(TryGetUserInfoAsync), true);
            }
            return result;
        }

        public async Task<OperationResult<VoidResponse>> TryFollowAsync()
        {
            if (UserProfileResponse.FollowedChanging)
                return new OperationResult<VoidResponse>(new OperationCanceledException());

            UserProfileResponse.FollowedChanging = true;
            NotifySourceChanged(nameof(TryFollowAsync), true);

            var hasFollowed = UserProfileResponse.HasFollowed;
            var request = new FollowModel(User.UserInfo, hasFollowed ? FollowType.UnFollow : FollowType.Follow, UserName);
            var result = await TaskHelper
                .TryRunTaskAsync(DitchClient.FollowAsync, request, OnDisposeCts.Token)
                .ConfigureAwait(false);

            if (result.IsSuccess)
                UserProfileResponse.HasFollowed = !hasFollowed;

            UserProfileResponse.FollowedChanging = false;
            CashManager.Update(UserProfileResponse);
            NotifySourceChanged(nameof(TryFollowAsync), true);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> TryUpdateUserPostsAsync(string username)
        {
            return await TaskHelper.TryRunTaskAsync(SteepshotApiClient.UpdateUserPostsAsync, username, OnDisposeCts.Token).ConfigureAwait(false);
        }

        public override void Clear(bool isNotify)
        {
            CashManager.Remove(UserProfileResponse);
            base.Clear(isNotify);
        }

        public async Task TryCheckSubscriptions()
        {
            OperationResult<SubscriptionsModel> response;
            do
            {
                response = await TaskHelper
                    .TryRunTaskAsync(SteepshotApiClient.CheckSubscriptionsAsync, User, OnDisposeCts.Token)
                    .ConfigureAwait(false);
                if (!response.IsSuccess)
                    await Task.Delay(5000).ConfigureAwait(false);
            } while (!response.IsSuccess);

            User.PushSettings = response.Result.EnumSubscriptions;
            SubscriptionsUpdated?.Invoke();
        }

        public async Task<OperationResult<object>> TrySubscribeForPushesAsync(PushNotificationsModel model)
        {
            var trxResp = await TaskHelper.TryRunTaskAsync(DitchClient.GetVerifyTransactionAsync, model, OnDisposeCts.Token).ConfigureAwait(false);

            if (!trxResp.IsSuccess)
                return new OperationResult<object>(trxResp.Exception);

            model.VerifyTransaction = JsonConvert.DeserializeObject<JObject>(trxResp.Result);
            return await TaskHelper.TryRunTaskAsync(SteepshotApiClient.SubscribeForPushesAsync, model, OnDisposeCts.Token);
        }

        #region IDisposable Support
        private bool _disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CashManager.Remove(UserProfileResponse);
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }
        #endregion
    }
}
