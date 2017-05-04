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
		public UserProfilePresenter(UserProfileView view) : base(view)
		{
		}

		private UserProfileResponse userData;

		private UserPostResponse postsData;

		public ObservableCollection<Post> UserPosts = new ObservableCollection<Post>();

		private bool _hasItems = true;
		private string _offsetUrl = string.Empty;

		public async Task<UserProfileResponse> GetUserInfo(string user, bool requireUpdate = false)
		{
			if (requireUpdate || userData == null)
			{
				var req = new UserProfileRequest(user) {SessionId = UserPrincipal.Instance.Cookie};
				var response = await Api.GetUserProfile(req);
				userData = response.Result;
			}
			return userData;
		}

		public async Task GetUserPosts()
		{
			if (!_hasItems)
				return;

			var req = new UserPostsRequest(userData.Username) { Offset = _offsetUrl, Limit=20, SessionId = UserPrincipal.Instance.Cookie };
			var response = await Api.GetUserPosts(req);

			if (response.Success)
			{
				var lastItem = response.Result.Results.Last();
				if (response.Result.Results.Count == 20)
					response.Result.Results.Remove(lastItem);
				else
					_hasItems = false;

				_offsetUrl = lastItem.Url;

				postsData = response.Result;

				foreach (var item in response.Result.Results)
				{
					UserPosts.Add(item);
				}
			}
		}

		public string GetPostsOffset()
		{
			if (postsData != null)
				return postsData.Offset;

			return null;
		}

        public async Task<OperationResult<VoteResponse>> Vote(Post post)
        {
            if (!UserPrincipal.Instance.IsAuthenticated)
                return new OperationResult<VoteResponse> { Errors = new List<string> { "Forbidden" }, Success = false };

            var voteRequest = new VoteRequest(UserPrincipal.Instance.CurrentUser.SessionId, !post.Vote, post.Url);
            return await Api.Vote(voteRequest);
        }

        public async Task<OperationResult<FollowResponse>> Follow()
        {
            var request = new FollowRequest(UserPrincipal.Instance.CurrentUser.SessionId, (userData.HasFollowed==0) ? FollowType.Follow : FollowType.UnFollow, userData.Username);
            var resp = await Api.Follow(request);
            if (resp.Errors.Count == 0)
            {
                userData.HasFollowed = (resp.Result.IsFollowed) ? 1 : 0;
            }
            return resp;
        }
    }
}
