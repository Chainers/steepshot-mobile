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
        private const int PostsCount = 40;
        public event Action PostsLoaded;
        public event Action PostsCleared;

        public UserProfilePresenter(string username)
        {
            _username = username;
        }

        public void ClearPosts()
        {
            Posts.Clear();
            _hasItems = true;
            _offsetUrl = string.Empty;
            PostsCleared?.Invoke();
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
                    _offsetUrl = string.Empty;
                    _hasItems = true;
                    Posts?.Clear();
                }

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
                    PostsLoaded?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
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
