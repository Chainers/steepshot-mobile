using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using Sweetshot.Library.Models.Common;
using System.Linq;

namespace Steepshot
{
	public class UserProfilePresenter : BasePresenter
	{
		private string _username;
		private UserPostResponse postsData;

		public ObservableCollection<Post> UserPosts = new ObservableCollection<Post>();

		private bool _hasItems = true;
		private string _offsetUrl = string.Empty;
		private const int postsCount = 40;
		public event VoidDelegate PostsLoaded;
		public event VoidDelegate PostsCleared;

		public UserProfilePresenter(UserProfileView view, string username) : base(view)
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
			var req = new UserProfileRequest(user) {SessionId = UserPrincipal.Instance.Cookie};
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
					Offset = _offsetUrl,
					Limit = postsCount,
					SessionId = UserPrincipal.Instance.Cookie
				};
				var response = await Api.GetUserPosts(req);

				if (response.Success)
				{
					var lastItem = response.Result.Results.Last();
					if (response.Result.Results.Count > postsCount / 2)
						response.Result.Results.Remove(lastItem);
					else
						_hasItems = false;

					_offsetUrl = lastItem.Url;

					postsData = response.Result;

					foreach (var item in response.Result.Results)
					{
						UserPosts.Add(item);
					}
					
					PostsLoaded?.Invoke();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
		}

        public async Task<OperationResult<VoteResponse>> Vote(Post post)
        {
            if (!UserPrincipal.Instance.IsAuthenticated)
                return new OperationResult<VoteResponse> { Errors = new List<string> { "Forbidden" }, Success = false };

            var voteRequest = new VoteRequest(UserPrincipal.Instance.CurrentUser.SessionId, !post.Vote, post.Url);
            return await Api.Vote(voteRequest);
        }

        public async Task<OperationResult<FollowResponse>> Follow(int hasFollowed)
        {
			var request = new FollowRequest(UserPrincipal.Instance.CurrentUser.SessionId, hasFollowed == 0 ? FollowType.Follow : FollowType.UnFollow, _username);
            var resp = await Api.Follow(request);
            if (resp.Errors.Count == 0)
            {
                //userData.HasFollowed = (resp.Result.IsFollowed) ? 1 : 0;
            }
            return resp;
        }
    }
}
