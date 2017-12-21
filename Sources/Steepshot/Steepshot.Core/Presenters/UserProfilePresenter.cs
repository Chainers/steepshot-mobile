using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Errors;

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
            var request = new UserPostsRequest(UserName)
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
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out error);
            } while (isNeedRepeat);

            return error;
        }


        public async Task<ErrorBase> TryGetUserInfo(string user)
        {
            return await TryRunTask(GetUserInfo, OnDisposeCts.Token, user);
        }

        private async Task<ErrorBase> GetUserInfo(CancellationToken ct, string user)
        {
            var req = new UserProfileRequest(user)
            {
                Login = User.Login,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            var response = await Api.GetUserProfile(req, ct);

            if (response.Success)
            {
                UserProfileResponse = response.Result;
                NotifySourceChanged();
            }
            return response.Error;
        }

        public async Task<ErrorBase> TryFollow()
        {
            if (UserProfileResponse.FollowedChanging)
                return null;

            UserProfileResponse.FollowedChanging = true;
            NotifySourceChanged();

            var error = await TryRunTask(Follow, OnDisposeCts.Token, UserProfileResponse);
            UserProfileResponse.FollowedChanging = false;
            NotifySourceChanged();
            return error;
        }

        private async Task<ErrorBase> Follow(CancellationToken ct, UserProfileResponse userProfileResponse)
        {
            var request = new FollowRequest(User.UserInfo, userProfileResponse.HasFollowed ? FollowType.UnFollow : FollowType.Follow, UserName);
            var response = await Api.Follow(request, ct);

            if (response.Success)
                userProfileResponse.HasFollowed = !userProfileResponse.HasFollowed;

            return response.Error;
        }
    }
}
