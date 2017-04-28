using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using Sweetshot.Library.Models.Common;

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

		public async Task<UserPostResponse> GetUserPosts()
		{
			var req = new UserPostsRequest(userData.Username) { SessionId = UserPrincipal.Instance.Cookie };
			var response = await Api.GetUserPosts(req);
			postsData = response.Result;
            UserPosts.Clear();
			foreach (var item in response.Result.Results)
			{
				UserPosts.Add(item);
			}

			return postsData;
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
