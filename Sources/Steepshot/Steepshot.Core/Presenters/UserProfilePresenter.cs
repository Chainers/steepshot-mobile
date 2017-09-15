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
        private bool _hasItems = true;
        private string _offsetUrl = string.Empty;
        private const int PostsCount = 20;

        public UserProfilePresenter(string username)
        {
            _username = username;
        }

        public void ClearPosts()
        {
            Posts.Clear();
            _hasItems = true;
            _offsetUrl = string.Empty;
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

                if (!_hasItems)
                    return errors;

                var req = new UserPostsRequest(_username)
                {
                    Login = User.Login,
                    Offset = _offsetUrl,
                    Limit = PostsCount,
                    ShowNsfw = User.IsNsfw,
                    ShowLowRated = User.IsLowRated
                };
                var response = await Api.GetUserPosts(req);
                errors = response?.Errors;
                if (response.Success && response.Result?.Results != null && response.Result?.Results.Count != 0)
                {
                    var lastItem = response.Result.Results.Last();
                    if (lastItem.Url != _offsetUrl)
                        response.Result.Results.Remove(lastItem);
                    else
                        _hasItems = false;

                    _offsetUrl = lastItem.Url;
                    Posts.AddRange(response.Result.Results);
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
