using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class UserProfilePresenter : BasePresenter
    {
        private readonly string _username;

        public List<Post> UserPosts = new List<Post>();

        private bool _hasItems = true;
        private string _offsetUrl = string.Empty;
        private const int PostsCount = 40;
        public event VoidDelegate PostsLoaded;
        public event VoidDelegate PostsCleared;

        public UserProfilePresenter(string username)
        {
            _username = username;
        }

        public void ClearPosts()
        {
            UserPosts.Clear();
            _hasItems = true;
            _offsetUrl = string.Empty;
            PostsCleared?.Invoke();
        }

        public async Task<UserProfileResponse> GetUserInfo(string user, bool requireUpdate = false)
        {
            var req = new UserProfileRequest(user)
            {
                Login = User.Login
            };
            var response = await Api.GetUserProfile(req);
            var userData = response.Result;
            return userData;
        }

        public async Task GetUserPosts(bool needRefresh = false)
        {
            try
            {
                if (needRefresh)
                {
                    _offsetUrl = string.Empty;
                    _hasItems = true;
                    UserPosts.Clear();
                }

                if (!_hasItems)
                    return;

                var req = new UserPostsRequest(_username)
                {
                    Login = User.Login,
                    Offset = _offsetUrl,
                    Limit = PostsCount
                };
                var response = await Api.GetUserPosts(req);

                if (response.Success && response.Result?.Results != null && response.Result?.Results.Count != 0)
                {
                    var lastItem = response.Result.Results.Last();
                    if (lastItem.Url != _offsetUrl)
                        response.Result.Results.Remove(lastItem);
                    else
                        _hasItems = false;

                    _offsetUrl = lastItem.Url;

                    foreach (var item in response.Result.Results)
                    {
                        UserPosts.Add(item);
                    }

                    PostsLoaded?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
        }

        public async Task<OperationResult<VoteResponse>> Vote(Post post)
        {
            if (!User.IsAuthenticated)
                return new OperationResult<VoteResponse> { Errors = new List<string> { "Forbidden" } };

            var voteRequest = new VoteRequest(User.UserInfo, !post.Vote, post.Url);

            return await Api.Vote(voteRequest);
        }

        public async Task<OperationResult<FollowResponse>> Follow(int hasFollowed)
        {
            var request = new FollowRequest(User.UserInfo, hasFollowed == 0 ? FollowType.Follow : FollowType.UnFollow, _username);
            var resp = await Api.Follow(request);
            if (resp.Errors.Count == 0)
            {
                //userData.HasFollowed = (resp.Result.IsFollowed) ? 1 : 0;
            }
            return resp;
        }
    }
}
