using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class UserProfilePresenter : BaseFeedPresenter
    {
        private readonly string _username;
        public event Action PostsLoaded;
        public event Action PostsCleared;
        private bool IsLastReaded = false;
        private string OffsetUrl = string.Empty;
        private const int ItemsLimit = 20;
        protected const int ServerMaxCount = 20;

        public UserProfilePresenter(string username)
        {
            _username = username;
        }

        public void ClearPosts()
        {
            Posts.Clear();
            PostsCleared?.Invoke();
            IsLastReaded = false;
            OffsetUrl = string.Empty;
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserInfo(string user, bool requireUpdate = false)
        {
            var req = new UserProfileRequest(user)
            {
                Login = User.Login
            };
            return await Api.GetUserProfile(req);
        }

        public async Task<List<string>> GetUserPosts(bool needRefresh = false)
        {
            List<string> errors = null;
            try
            {
                if (needRefresh)
                {
                    OffsetUrl = string.Empty;
                    IsLastReaded = false;
                    Posts?.Clear();
                }

                if (IsLastReaded)
                    return errors;

                var req = new UserPostsRequest(_username)
                {
                    Login = User.Login,
                    Offset = OffsetUrl,
                    Limit = ItemsLimit,
                    ShowNsfw = User.IsNsfw,
                    ShowLowRated = User.IsLowRated
                };
                var response = await Api.GetUserPosts(req);
                errors = response?.Errors;

                if (response.Success)
                {
                    var voters = response.Result.Results;
                    if (voters.Count > 0)
                    {
                        Posts.AddRange(string.IsNullOrEmpty(OffsetUrl) ? voters : voters.Skip(1));
                        OffsetUrl = voters.Last().Url;
                    }

                    PostsLoaded?.Invoke();
                    if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                        IsLastReaded = true;
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return errors;
        }

        public async Task<OperationResult<FollowResponse>> Follow(int hasFollowed)
        {
            var request = new FollowRequest(User.UserInfo, hasFollowed == 0 ? FollowType.Follow : FollowType.UnFollow, _username);
            var resp = await Api.Follow(request);
            if (resp.Errors.Count == 0)
            {
                //userData.HasFollowed = (resp.Result.IsSuccess) ? 1 : 0;
            }
            return resp;
        }
    }
}
