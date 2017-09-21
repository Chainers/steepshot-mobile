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
        private const int ItemsLimit = 20;
        private bool IsLastReaded = false;
        private string OffsetUrl = string.Empty;
        protected const int ServerMaxCount = 20;

        public UserProfilePresenter(string username)
        {
            _username = username;
        }

        public void ClearPosts()
        {
            Posts.Clear();
            IsLastReaded = false;
            OffsetUrl = string.Empty;
        }

        public Task<OperationResult<UserProfileResponse>> GetUserInfo(string user)
        {
            var req = new UserProfileRequest(user)
            {
                Login = User.Login
            };
            return Api.GetUserProfile(req);
        }

        public async Task<List<string>> GetUserPosts(bool needRefresh = false)
        {
            List<string> errors = null;
            try
            {
                if (needRefresh)
                    ClearPosts();

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

        public Task<OperationResult<FollowResponse>> Follow(int hasFollowed)
        {
            var request = new FollowRequest(User.UserInfo, hasFollowed == 0 ? FollowType.Follow : FollowType.UnFollow, _username);
            return Api.Follow(request);
        }
    }
}
