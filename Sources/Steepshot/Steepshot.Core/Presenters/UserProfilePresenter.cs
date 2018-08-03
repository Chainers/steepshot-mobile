﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public sealed class UserProfilePresenter : BasePostPresenter
    {
        private const int ItemsLimit = 18;

        public string UserName { get; set; }

        public UserProfileResponse UserProfileResponse { get; private set; }


        public async Task<Exception> TryLoadNextPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextPosts);
        }

        private async Task<Exception> LoadNextPosts(CancellationToken ct)
        {
            var request = new UserPostsModel(UserName)
            {
                Login = AppSettings.User.Login,
                Offset = OffsetUrl,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated
            };

            Exception error;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetUserPosts(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out error, nameof(TryLoadNextPosts));
            } while (isNeedRepeat);

            return error;
        }


        public async Task<Exception> TryGetUserInfo(string user)
        {
            return await TryRunTask(GetUserInfo, OnDisposeCts.Token, user);
        }

        private async Task<Exception> GetUserInfo(string user, CancellationToken ct)
        {
            var req = new UserProfileModel(user)
            {
                Login = AppSettings.User.Login,
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated
            };
            var response = await Api.GetUserProfile(req, ct);

            if (response.IsSuccess)
            {
                UserProfileResponse = response.Result;
                CashPresenterManager.Add(UserProfileResponse);
                NotifySourceChanged(nameof(TryGetUserInfo), true);
            }
            return response.Error;
        }


        public async Task<Exception> TryFollow()
        {
            if (UserProfileResponse.FollowedChanging)
                return null;

            UserProfileResponse.FollowedChanging = true;
            NotifySourceChanged(nameof(TryFollow), true);

            var error = await TryRunTask(Follow, OnDisposeCts.Token, UserProfileResponse);
            UserProfileResponse.FollowedChanging = false;
            CashPresenterManager.Update(UserProfileResponse);
            NotifySourceChanged(nameof(TryFollow), true);
            return error;
        }

        private async Task<Exception> Follow(UserProfileResponse userProfileResponse, CancellationToken ct)
        {
            var hasFollowed = userProfileResponse.HasFollowed;
            var request = new FollowModel(AppSettings.User.UserInfo, hasFollowed ? FollowType.UnFollow : FollowType.Follow, UserName);
            var response = await Api.Follow(request, ct);

            if (response.IsSuccess)
                userProfileResponse.HasFollowed = !hasFollowed;

            return response.Error;
        }


        public async Task<Exception> TryUpdateUserProfile(UpdateUserProfileModel model, UserProfileResponse currentProfile)
        {
            var error = await TryRunTask(UpdateUserProfile, OnDisposeCts.Token, model);
            if (error != null)
            {
                NotifySourceChanged(nameof(TryUpdateUserProfile), false);
            }
            else
            {
                //TODO:KOA: Looks like it is work for AutoMapper
                currentProfile.About = model.About;
                currentProfile.Name = model.Name;
                currentProfile.Location = model.Location;
                currentProfile.Website = model.Website;
                currentProfile.ProfileImage = model.ProfileImage;
                NotifySourceChanged(nameof(TryUpdateUserProfile), false);
            }

            return error;
        }

        private async Task<Exception> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            var response = await Api.UpdateUserProfile(model, ct);
            return response.Error;
        }

        public override void Clear(bool isNotify = true)
        {
            CashPresenterManager.Remove(UserProfileResponse);
            base.Clear(isNotify);
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
