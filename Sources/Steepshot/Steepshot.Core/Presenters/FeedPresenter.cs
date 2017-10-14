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
    public sealed class FeedPresenter : BasePostPresenter
    {
        private readonly bool _isFeed;
        public PostType PostType = PostType.Top;
        private const int ItemsLimit = 20;
        public string Tag;

        public FeedPresenter(bool isFeed)
        {
            _isFeed = isFeed;
        }

        public async Task<List<string>> TryLoadNextTopPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<List<string>> LoadNextTopPosts(CancellationToken ct)
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
                response = await Api.GetUserRecentPosts(f, ct);
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
                response = await Api.GetPosts(postrequest, ct);
            }
            if (response.Success)
            {
                var results = response.Result.Results;
                if (results.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? results : results.Skip(1));

                    OffsetUrl = results.Last().Url;
                }
                if (results.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }


        public async Task<List<string>> TryLoadNextSearchedPosts()
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNextSearchedPosts);
        }

        private async Task<List<string>> LoadNextSearchedPosts(CancellationToken ct)
        {
            var postrequest = new PostsByCategoryRequest(PostType, Tag)
            {
                Login = User.Login,
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            var response = await Api.GetPostsByCategory(postrequest, ct);
            if (response.Success)
            {
                var results = response.Result.Results;
                if (results.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? results : results.Skip(1));

                    OffsetUrl = results.Last().Url;
                }
                if (results.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }
    }
}
