using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class PreSearchPresenter : BaseFeedPresenter
    {
        private CancellationTokenSource _cts;
        public PostType PostType = PostType.Top;
        private bool _hasItems = true;
        public bool HasItems
        {
            get
            {
                return _hasItems;
            }
        }
        private string _offsetUrl = string.Empty;
        private const int PostsCount = 19;
        public string Tag;

        public void ClearPosts()
        {
            Posts.Clear();
            _hasItems = true;
            _offsetUrl = string.Empty;
        }

        public async Task<List<string>> GetTopPosts(bool clearOld = false)
        {
            List<string> errors = null;
            try
            {
                if (clearOld)
                    ClearPosts();
                if (!_hasItems)
                    return errors;
                try
                {
                    _cts?.Cancel();
                }
                catch (ObjectDisposedException)
                {

                }

                using (_cts = new CancellationTokenSource())
                {
                    var postrequest = new PostsRequest(PostType)
                    {
                        Login = User.Login,
                        Limit = PostsCount,
                        Offset = _offsetUrl,
                        ShowNsfw = User.IsNsfw,
                        ShowLowRated = User.IsLowRated
                    };
                    var response = await Api.GetPosts(postrequest, _cts);

                    errors = response.Errors;
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
                            Posts.AddRange(response.Result.Results);
                        }
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return errors;
        }

        public async Task<List<string>> GetSearchedPosts(bool clearOld = false)
        {
            List<string> errors = null;
            if (clearOld)
                ClearPosts();
            if (!_hasItems)
                return errors;
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
                    var postrequest = new PostsByCategoryRequest(PostType, Tag)
                    {
                        Login = User.Login,
                        Limit = PostsCount,
                        Offset = _offsetUrl,
                        ShowNsfw = User.IsNsfw,
                        ShowLowRated = User.IsLowRated
                    };

                    var posts = await Api.GetPostsByCategory(postrequest, _cts);
                    errors = posts.Errors;
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
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return errors;
        }
    }
}
