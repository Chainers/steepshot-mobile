using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class FeedPresenter : BaseFeedPresenter
    {
        private readonly bool _isFeed;
        public PostType PostType = PostType.Top;
        private const int ItemsLimit = 20;
        public string Tag;

        public FeedPresenter(bool isFeed)
        {
            _isFeed = isFeed;
        }

        public async Task<List<string>> TryLoadNextTopPosts(bool needRefresh = false)
        {
            if (needRefresh)
                ClearPosts();

            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<List<string>> LoadNextTopPosts(CancellationTokenSource cts)
        {
            OperationResult<UserPostResponse> response;
            if (_isFeed)
            {
                var f = new CensoredNamedRequestWithOffsetLimitFields
                {
                    Login = User.Login,
                    Limit = ItemsLimit,
                    Offset = OffsetUrl,
                    ShowNsfw = User.IsNsfw,
                    ShowLowRated = User.IsLowRated
                };
                response = await Api.GetUserRecentPosts(f, cts);
            }
            else
            {
                var postrequest = new PostsRequest(PostType)
                {
                    Login = User.Login,
                    Limit = ItemsLimit,
                    Offset = OffsetUrl,
                    ShowNsfw = User.IsNsfw,
                    ShowLowRated = User.IsLowRated
                };
                response = await Api.GetPosts(postrequest, cts);
            }
            if (response.Success)
            {
                var results = response.Result.Results;
                if (results.Count > 0)
                {
                    lock (Posts)
                        Posts.AddRange(string.IsNullOrEmpty(OffsetUrl) ? results : results.Skip(1));

                    OffsetUrl = results.Last().Url;
                }
                if (results.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }


        public async Task<List<string>> TryLoadNextSearchedPosts(bool needRefresh = false)
        {
            if (needRefresh)
                ClearPosts();

            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNextSearchedPosts);
        }

        private async Task<List<string>> LoadNextSearchedPosts(CancellationTokenSource cts)
        {
            var postrequest = new PostsByCategoryRequest(PostType, Tag)
            {
                Login = User.Login,
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            var response = await Api.GetPostsByCategory(postrequest, cts);
            if (response.Success)
            {
                var results = response.Result.Results;
                if (results.Count > 0)
                {
                    lock (Posts)
                        Posts.AddRange(string.IsNullOrEmpty(OffsetUrl) ? results : results.Skip(1));

                    OffsetUrl = results.Last().Url;
                }
                if (results.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }
    }
}
