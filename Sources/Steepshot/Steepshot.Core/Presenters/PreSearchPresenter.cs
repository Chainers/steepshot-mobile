using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class PreSearchPresenter : BasePostPresenter
    {
        public PostType PostType = PostType.Top;
        private const int ItemsLimit = 20;
        public string Tag;

        public async Task<List<string>> TryLoadNextTopPosts(bool clearOld = false)
        {
            if (clearOld)
                Clear();

            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<List<string>> LoadNextTopPosts(CancellationTokenSource cts)
        {
            var postrequest = new PostsRequest(PostType)
            {
                Login = User.Login,
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            var response = await Api.GetPosts(postrequest, cts);

            if (response.Success)
            {
                var posts = response.Result.Results;
                if (posts.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? posts : posts.Skip(1));

                    OffsetUrl = posts.Last().Url;
                }

                if (posts.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }

        public async Task<List<string>> TryGetSearchedPosts(bool clearOld = false)
        {
            if (clearOld)
                Clear();

            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(GetSearchedPosts);
        }

        private async Task<List<string>> GetSearchedPosts(CancellationTokenSource cts)
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
                var posts = response.Result.Results;
                if (posts.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? posts : posts.Skip(1));

                    OffsetUrl = posts.Last().Url;
                }

                if (posts.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }

            return response.Errors;
        }
    }
}
