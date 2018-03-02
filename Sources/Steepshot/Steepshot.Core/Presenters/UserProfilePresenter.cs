using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Presenters
{
    public sealed class UserProfilePresenter : BasePostPresenter
    {
        private const int ItemsLimit = 18;

        public string UserName { get; set; }

        public UserProfileResponse UserProfileResponse { get; private set; }


        public async Task<ErrorBase> TryLoadNextPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextPosts);
        }

        private async Task<ErrorBase> LoadNextPosts(CancellationToken ct)
        {
            var request = new UserPostsModel(UserName)
            {
                Login = User.Login,
                Offset = OffsetUrl,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            ErrorBase error;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetUserPosts(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out error, nameof(TryLoadNextPosts));
            } while (isNeedRepeat);

            return error;
        }


        public async Task<ErrorBase> TryGetUserInfo(string user)
        {
            return await TryRunTask(GetUserInfo, OnDisposeCts.Token, user);
        }

        private async Task<ErrorBase> GetUserInfo(string user, CancellationToken ct)
        {
            var req = new UserProfileModel(user)
            {
                Login = User.Login,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            var response = await Api.GetUserProfile(req, ct);

            if (response.IsSuccess)
            {
                UserProfileResponse = response.Result;
                NotifySourceChanged(nameof(TryGetUserInfo), true);
            }
            return response.Error;
        }


        public async Task<ErrorBase> TryFollow()
        {
            if (UserProfileResponse.FollowedChanging)
                return null;

            UserProfileResponse.FollowedChanging = true;
            NotifySourceChanged(nameof(TryFollow), true);

            var error = await TryRunTask(Follow, OnDisposeCts.Token, UserProfileResponse);
            UserProfileResponse.FollowedChanging = false;
            NotifySourceChanged(nameof(TryFollow), true);
            return error;
        }

        private async Task<ErrorBase> Follow(UserProfileResponse userProfileResponse, CancellationToken ct)
        {
            var request = new FollowModel(User.UserInfo, userProfileResponse.HasFollowed ? FollowType.UnFollow : FollowType.Follow, UserName);
            var response = await Api.Follow(request, ct);

            if (response.IsSuccess)
                userProfileResponse.HasFollowed = !userProfileResponse.HasFollowed;

            return response.Error;
        }


        public async Task<ErrorBase> TryUpdateUserProfile(UpdateUserProfileModel model, UserProfileResponse currentProfile)
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

        private async Task<ErrorBase> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            var response = await Api.UpdateUserProfile(model, ct);
            return response.Error;
        }

        public async Task<ErrorBase> TrySubscribeForPushes(string playerId, PushSubscription[] subscriptions = null)
        {
            var model = new PushNotificationsModel(User.UserInfo, playerId, subscriptions, true);
            var error = await TryRunTask(SubscribeForPushes, OnDisposeCts.Token, model);
            return error;
        }

        private async Task<ErrorBase> SubscribeForPushes(PushNotificationsModel model, CancellationToken ct)
        {
            var response = await Api.SubscribeForPushes(model, OnDisposeCts.Token);
            return response.Error;
        }
    }
}
