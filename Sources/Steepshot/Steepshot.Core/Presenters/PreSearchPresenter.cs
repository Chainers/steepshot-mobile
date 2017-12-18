using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.Helpers;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public sealed class PreSearchPresenter : BasePostPresenter
    {
        public PostType PostType = PostType.Hot;
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
            var request = new PostsRequest(PostType)
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            List<string> errors;
            OperationResult<ListResponce<Post>> response;
            do
            {
                response = await Api.GetPosts(request, ct);
            } while (ResponseProcessing(response, ItemsLimit, out errors));

            return errors;
        }

        public async Task<List<string>> TryGetSearchedPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(GetSearchedPosts);
        }

        private async Task<List<string>> GetSearchedPosts(CancellationToken ct)
        {
            var request = new PostsByCategoryRequest(PostType, Tag.TagToEn())
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            List<string> errors;
            OperationResult<ListResponce<Post>> response;
            bool isNeedRepeat;
            do
            {
                response = await Api.GetPostsByCategory(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out errors);
            } while (isNeedRepeat);

            return errors;
        }
    }
}
