using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Presenters
{
    public sealed class PreSearchPresenter : BasePostPresenter
    {
        public PostType PostType = PostType.Hot;
        private const int ItemsLimit = 18;
        public string Tag;

        public async Task<Exception> TryLoadNextTopPostsAsync()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTaskAsync(LoadNextTopPostsAsync).ConfigureAwait(false);
        }

        private async Task<Exception> LoadNextTopPostsAsync(CancellationToken ct)
        {
            var request = new PostsModel(PostType)
            {
                Login = AppSettings.User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetPostsAsync(request, ct).ConfigureAwait(false);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextTopPostsAsync));
            } while (isNeedRepeat);

            return exception;
        }

        public async Task<Exception> TryGetSearchedPostsAsync()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTaskAsync(GetSearchedPostsAsync).ConfigureAwait(false);
        }

        private async Task<Exception> GetSearchedPostsAsync(CancellationToken ct)
        {
            var request = new PostsByCategoryModel(PostType, Tag.TagToEn())
            {
                Login = AppSettings.User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetPostsByCategoryAsync(request, ct).ConfigureAwait(false);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryGetSearchedPostsAsync));
            } while (isNeedRepeat);

            return exception;
        }
    }
}
