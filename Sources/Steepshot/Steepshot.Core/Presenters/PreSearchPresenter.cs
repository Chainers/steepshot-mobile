using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Errors;

namespace Steepshot.Core.Presenters
{
    public sealed class PreSearchPresenter : BasePostPresenter
    {
        public PostType PostType = PostType.Hot;
        private const int ItemsLimit = 18;
        public string Tag;

        public async Task<ErrorBase> TryLoadNextTopPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<ErrorBase> LoadNextTopPosts(CancellationToken ct)
        {
            var request = new PostsRequest(PostType)
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            ErrorBase error;
            OperationResult<ListResponce<Post>> response;
            do
            {
                response = await Api.GetPosts(request, ct);
            } while (ResponseProcessing(response, ItemsLimit, out error, nameof(TryLoadNextTopPosts)));

            return error;
        }

        public async Task<ErrorBase> TryGetSearchedPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(GetSearchedPosts);
        }

        private async Task<ErrorBase> GetSearchedPosts(CancellationToken ct)
        {
            var request = new PostsByCategoryRequest(PostType, Tag.TagToEn())
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            ErrorBase error;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetPostsByCategory(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out error, nameof(TryGetSearchedPosts));
            } while (isNeedRepeat);

            return error;
        }
    }
}
