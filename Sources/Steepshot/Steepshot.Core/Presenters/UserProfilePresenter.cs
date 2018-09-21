using System;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<Exception> TryLoadNextPostsAsync()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTaskAsync(LoadNextPostsAsync).ConfigureAwait(false);
        }

        private async Task<Exception> LoadNextPostsAsync(CancellationToken ct)
        {
            var request = new UserPostsModel(UserName)
            {
                Login = AppSettings.User.Login,
                Offset = OffsetUrl,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetUserPostsAsync(request, ct).ConfigureAwait(false);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextPostsAsync));
            } while (isNeedRepeat);

            return exception;
        }


        public async Task<Exception> TryGetUserInfoAsync(string user)
        {
            return await TryRunTaskAsync(GetUserInfoAsync, OnDisposeCts.Token, user).ConfigureAwait(false);
        }

        private async Task<Exception> GetUserInfoAsync(string user, CancellationToken ct)
        {
            var req = new UserProfileModel(user)
            {
                Login = AppSettings.User.Login,
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated
            };
            var response = await Api.GetUserProfileAsync(req, ct).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                UserProfileResponse = response.Result;
                CashPresenterManager.Add(UserProfileResponse);
                NotifySourceChanged(nameof(TryGetUserInfoAsync), true);
            }
            return response.Exception;
        }


        public async Task<Exception> TryFollowAsync()
        {
            if (UserProfileResponse.FollowedChanging)
                return null;

            UserProfileResponse.FollowedChanging = true;
            NotifySourceChanged(nameof(TryFollowAsync), true);

            var exception = await TryRunTaskAsync(FollowAsync, OnDisposeCts.Token, UserProfileResponse).ConfigureAwait(false);
            UserProfileResponse.FollowedChanging = false;
            CashPresenterManager.Update(UserProfileResponse);
            NotifySourceChanged(nameof(TryFollowAsync), true);
            return exception;
        }

        private async Task<Exception> FollowAsync(UserProfileResponse userProfileResponse, CancellationToken ct)
        {
            var hasFollowed = userProfileResponse.HasFollowed;
            var request = new FollowModel(AppSettings.User.UserInfo, hasFollowed ? FollowType.UnFollow : FollowType.Follow, UserName);
            var response = await Api.FollowAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccess)
                userProfileResponse.HasFollowed = !hasFollowed;

            return response.Exception;
        }


        public async Task<Exception> TryUpdateUserProfileAsync(UpdateUserProfileModel model, UserProfileResponse currentProfile)
        {
            var exception = await TryRunTaskAsync(UpdateUserProfileAsync, OnDisposeCts.Token, model).ConfigureAwait(false);
            if (exception != null)
            {
                NotifySourceChanged(nameof(TryUpdateUserProfileAsync), false);
            }
            else
            {
                //TODO:KOA: Looks like it is work for AutoMapper
                currentProfile.About = model.About;
                currentProfile.Name = model.Name;
                currentProfile.Location = model.Location;
                currentProfile.Website = model.Website;
                currentProfile.ProfileImage = model.ProfileImage;
                NotifySourceChanged(nameof(TryUpdateUserProfileAsync), false);
            }

            return exception;
        }

        private async Task<Exception> UpdateUserProfileAsync(UpdateUserProfileModel model, CancellationToken ct)
        {
            var response = await Api.UpdateUserProfileAsync(model, ct).ConfigureAwait(false);
            return response.Exception;
        }

        public async Task<Exception> TryUpdateUserPostsAsync(string username)
        {
            return await TryRunTaskAsync(UpdateUserPostsAsync, OnDisposeCts.Token, username).ConfigureAwait(false);
        }

        private async Task<Exception> UpdateUserPostsAsync(string username, CancellationToken ct)
        {
            var response = await Api.UpdateUserPostsAsync(username, ct).ConfigureAwait(false);
            return response.Exception;
        }

        public override void Clear(bool isNotify = true)
        {
            CashPresenterManager.Remove(UserProfileResponse);
            base.Clear(isNotify);
        }

        public async void TryCheckSubscriptions()
        {
            OperationResult<SubscriptionsModel> response;
            do
            {
                response = await TryRunTaskAsync<SubscriptionsModel>(CheckSubscriptionsAsync, CancellationToken.None).ConfigureAwait(false);
                if (!response.IsSuccess)
                    await Task.Delay(5000).ConfigureAwait(false);
            } while (!response.IsSuccess);

            AppSettings.User.PushSettings = response.Result.EnumSubscriptions;
            SubscriptionsUpdated?.Invoke();
        }

        private async Task<OperationResult<SubscriptionsModel>> CheckSubscriptionsAsync(CancellationToken ct)
        {
            var response = await Api.CheckSubscriptionsAsync(AppSettings.User, ct).ConfigureAwait(false);
            return response;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CashPresenterManager.Remove(UserProfileResponse);
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                _disposedValue = true;
            }
        }
        #endregion
    }
}
