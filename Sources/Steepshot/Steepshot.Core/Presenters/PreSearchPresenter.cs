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
        private const int ItemsLimit = 18;
        public string Tag;

        public async Task<List<string>> TryLoadNextTopPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<List<string>> LoadNextTopPosts(CancellationToken ct)
        {
            var postrequest = new PostsRequest(PostType)
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };
            var response = await Api.GetPosts(postrequest, ct);
            if (response == null)
                return null;

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
                NotifySourceChanged();
            }
            return response.Errors;
        }

        public async Task<List<string>> TryGetSearchedPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(GetSearchedPosts);
        }

        private async Task<List<string>> GetSearchedPosts(CancellationToken ct)
        {
            var postrequest = new PostsByCategoryRequest(PostType, Tag)
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            var response = await Api.GetPostsByCategory(postrequest, ct);
            if (response == null)
                return null;

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
                NotifySourceChanged();
            }

            return response.Errors;
        }
    }
}
