using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public sealed class UserProfilePresenter : BasePostPresenter
    {
        private const int ItemsLimit = 18;

        public string UserName { get; set; }

        public UserProfileResponse UserProfileResponse { get; private set; }
        
        public async Task<List<string>> TryLoadNextPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextPosts);
        }

        private async Task<List<string>> LoadNextPosts(CancellationToken ct)
        {
            var request = new UserPostsRequest(UserName)
            {
                Login = User.Login,
                Offset = OffsetUrl,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            List<string> errors;
            OperationResult<UserPostResponse> response;
            do
            {
                response = await Api.GetUserPosts(request, ct);
            } while (ResponseProcessing(response, ItemsLimit, out errors));

            return errors;
        }


        public async Task<List<string>> TryGetUserInfo(string user)
        {
            return await TryRunTask(GetUserInfo, OnDisposeCts.Token, user);
        }

        private async Task<List<string>> GetUserInfo(CancellationToken ct, string user)
        {
            var req = new UserProfileRequest(user)
            {
                Login = User.Login,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            var response = await Api.GetUserProfile(req, ct);
            if (response == null)
                return null;

            if (response.Success)
            {
                UserProfileResponse = response.Result;
                NotifySourceChanged();
            }
            return response.Errors;
        }

        public async Task<List<string>> TryFollow()
        {
            if (UserProfileResponse.FollowedChanging)
                return null;

            UserProfileResponse.FollowedChanging = true;
            NotifySourceChanged();

            var errors = await TryRunTask(Follow, OnDisposeCts.Token, UserProfileResponse);
            UserProfileResponse.FollowedChanging = false;
            NotifySourceChanged();
            return errors;
        }

        private async Task<List<string>> Follow(CancellationToken ct, UserProfileResponse userProfileResponse)
        {
            var request = new FollowRequest(User.UserInfo, userProfileResponse.HasFollowed ? FollowType.UnFollow : FollowType.Follow, UserName);
            var response = await Api.Follow(request, ct);
            if (response == null)
                return null;

            if (response.Success)
                userProfileResponse.HasFollowed = !userProfileResponse.HasFollowed;

            return response.Errors;
        }
    }
}
