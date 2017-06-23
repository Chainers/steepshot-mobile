using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class FeedPresenter : BasePresenter
	{
		public FeedPresenter(FeedView view):base(view) {}
		public event VoidDelegate PostsLoaded;
		public event VoidDelegate PostsCleared;
		public ObservableCollection<Post> Posts = new ObservableCollection<Post>();

		private PostType type = PostType.Top;

		private bool _hasItems = true;
		private string _offsetUrl = string.Empty;
		private const int postsCount = 20;

		public PostType GetCurrentType()
		{
			return type;
		}

		public void ViewLoad()
		{
			if (Posts.Count == 0)
				Task.Run(() => GetTopPosts(type, true));
		}

		public bool processing = false;

		public void ClearPosts()
		{
			Posts.Clear();
			_hasItems = true;
			_offsetUrl = string.Empty;
			PostsCleared?.Invoke();
		}

		public async Task GetTopPosts(PostType type, bool clearOld = false)
		{
			if (!_hasItems)
				return;
			
			this.type = type;
			processing = true;

			var postrequest = new PostsRequest(type)
			{
                SessionId = UserPrincipal.Instance.Cookie,
                Limit = postsCount,
				Offset = _offsetUrl
			};

			var response = await Api.GetPosts(postrequest);
			//TODO:KOA -- Errors not processed
			if (response.Success)
			{
				if (response.Result.Results.Count != 0)
				{
					var lastItem = response.Result.Results.Last();
					if (response.Result.Results.Count == postsCount)
						response.Result.Results.Remove(lastItem);
					else
						_hasItems = false;

					_offsetUrl = lastItem.Url;

					if (clearOld)
					{
						Posts.Clear();
					}
					foreach (var item in response.Result.Results)
					{
						Posts.Add(item);
					}
				}
			}
			processing = false;
			PostsLoaded?.Invoke();
		}

		public async Task GetSearchedPosts(string query)
		{
			if (!_hasItems)
				return;
			processing = true;
			var postrequest = new PostsByCategoryRequest(type, query)
			{
				SessionId = UserPrincipal.Instance.Cookie,
                Limit = postsCount,
				Offset = _offsetUrl
			};

			var posts = await Api.GetPostsByCategory(postrequest);
			//TODO:KOA -- Errors not processed
			if (posts.Success)
			{
				if (posts.Result.Results.Count != 0)
				{
					var lastItem = posts.Result.Results.Last();
					if (posts.Result.Results.Count == postsCount)
						posts.Result.Results.Remove(lastItem);
					else
						_hasItems = false;

					_offsetUrl = lastItem.Url;

					Posts.Clear();

					foreach (var item in posts.Result.Results)
					{
						Posts.Add(item);
					}
				}
			}
			processing = false;
			PostsLoaded?.Invoke();
		}

		public async Task<OperationResult<VoteResponse>> Vote(Post post)
		{
			if (!UserPrincipal.Instance.IsAuthenticated)
				return new OperationResult<VoteResponse> { Errors = new List<string> { "Forbidden" }, Success = false };

			var voteRequest = new VoteRequest(UserPrincipal.Instance.CurrentUser.SessionId, !post.Vote, post.Url);
			return await Api.Vote(voteRequest);
		}

        public async Task<OperationResult<LogoutResponse>> Logout()
        {
            var request = new LogoutRequest(UserPrincipal.Instance.CurrentUser.SessionId);
            return await Api.Logout(request);
        }
    }
}
