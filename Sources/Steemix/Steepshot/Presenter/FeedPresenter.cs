using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class FeedPresenter : BasePresenter
	{
		public FeedPresenter(FeedView view, bool isFeed) : base(view)
		{
			_isFeed = isFeed;
		}
		private bool _isFeed;
		public event VoidDelegate PostsLoaded;
		public event VoidDelegate PostsCleared;
		public ObservableCollection<Post> Posts = new ObservableCollection<Post>();
		private CancellationTokenSource cts;
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
			try
			{
				if (!_hasItems)
					return;
				try
				{
					cts?.Cancel();
				}
				catch (ObjectDisposedException)
				{

				}

				using (cts = new CancellationTokenSource())
				{
					this.type = type;
					processing = true;

					OperationResult<UserPostResponse> response;
					if (_isFeed)
					{
						var f = new UserRecentPostsRequest(UserPrincipal.Instance.Cookie)
						{
							Limit = postsCount,
							Offset = _offsetUrl
						};
						response = await Api.GetUserRecentPosts(f);
					}
					else
					{
						var postrequest = new PostsRequest(type)
						{
							SessionId = UserPrincipal.Instance.Cookie,
							Limit = postsCount,
							Offset = _offsetUrl
						};
						response = await Api.GetPosts(postrequest, cts);
					}
					//TODO:KOA -- Errors not processed
					if (response.Success && response?.Result?.Results != null)
					{
						if (response.Result.Results.Count != 0)
						{
							var lastItem = response.Result.Results.Last();
							if (lastItem.Url != _offsetUrl)
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
						PostsLoaded?.Invoke();
					}
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				processing = false;
			}
		}

		public async Task GetSearchedPosts(string query)
		{
			if (!_hasItems)
				return;

			try
			{
				cts?.Cancel();
			}
			catch (ObjectDisposedException)
			{
				
			}
			try
			{
				using (cts = new CancellationTokenSource())
				{
					processing = true;
					var postrequest = new PostsByCategoryRequest(type, query)
					{
						SessionId = UserPrincipal.Instance.Cookie,
						Limit = postsCount,
						Offset = _offsetUrl
					};

					var posts = await Api.GetPostsByCategory(postrequest, cts);
					//TODO:KOA -- Errors not processed
					if (posts.Success && posts?.Result?.Results != null)
					{
						if (posts.Result.Results.Count != 0)
						{
							var lastItem = posts.Result.Results.Last();
							if (lastItem.Url != _offsetUrl)
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
						PostsLoaded?.Invoke();
					}
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				processing = false;
			}
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
