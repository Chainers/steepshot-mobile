using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class FeedPresenter : BaseFeedPresenter
    {
        private readonly bool _isFeed;
        public event VoidDelegate PostsLoaded;
        public event VoidDelegate PostsCleared;
        private CancellationTokenSource _cts;
        private PostType _type = PostType.Top;
        public bool Processing;
        private bool _hasItems = true;
		public bool HasItems
		{
			get
			{
				return _hasItems;
			}
		}
        private string _offsetUrl = string.Empty;
        private const int PostsCount = 20;
        public string Tag;

        public FeedPresenter(bool isFeed)
        {
            _isFeed = isFeed;
        }

        public PostType GetCurrentType()
        {
            return _type;
        }

        public async Task ViewLoad()
        {
			if (Posts.Count == 0)
				await GetTopPosts(_type, true);
        }
        
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
                if (!_hasItems || Processing)
                    return;
                try
                {
                    _cts?.Cancel();
                }
                catch (ObjectDisposedException)
                {

                }

                using (_cts = new CancellationTokenSource())
                {
                    _type = type;
                    Processing = true;

                    OperationResult<UserPostResponse> response;
                    if (_isFeed)
                    {
                        var f = new CensoredPostsRequests
                        {
                            Login = User.Login,
                            Limit = PostsCount,
                            Offset = _offsetUrl,
                            ShowNsfw = User.IsNsfw,
                            ShowLowRated = User.IsLowRated
                        };
                        response = await Api.GetUserRecentPosts(f);
                    }
                    else
                    {
                        var postrequest = new PostsRequest(type)
                        {
                            Login = User.Login,
                            Limit = PostsCount,
                            Offset = _offsetUrl,
							ShowNsfw = User.IsNsfw,
							ShowLowRated = User.IsLowRated
                        };
                        response = await Api.GetPosts(postrequest, _cts);
                    }
                    //TODO:KOA -- Errors not processed
                    if (response.Success && response.Result?.Results != null)
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
                    }
                    PostsLoaded?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
            finally
            {
                Processing = false;
            }
        }

        public async Task GetSearchedPosts()
        {
            if (!_hasItems)
                return;

            try
            {
                _cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            try
            {
                using (_cts = new CancellationTokenSource())
                {
                    Processing = true;
                    var postrequest = new PostsByCategoryRequest(_type, Tag)
                    {
                        Login = User.Login,
                        Limit = PostsCount,
                        Offset = _offsetUrl,
						ShowNsfw = User.IsNsfw,
						ShowLowRated = User.IsLowRated
                    };

                    var posts = await Api.GetPostsByCategory(postrequest, _cts);
                    //TODO:KOA -- Errors not processed
                    if (posts.Success && posts.Result?.Results != null)
                    {
                        if (posts.Result.Results.Count != 0)
                        {
                            var lastItem = posts.Result.Results.Last();
                            if (lastItem.Url != _offsetUrl)
                                posts.Result.Results.Remove(lastItem);
                            else
                                _hasItems = false;

                            _offsetUrl = lastItem.Url;
                            Posts.AddRange(posts.Result.Results);
                        }
                        PostsLoaded?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
            finally
            {
                Processing = false;
            }
        }
    }
}