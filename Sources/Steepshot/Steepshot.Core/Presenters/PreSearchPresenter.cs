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

        public async Task<Exception> TryLoadNextTopPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<Exception> LoadNextTopPosts(CancellationToken ct)
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
                var response = await Api.GetPosts(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextTopPosts));
            } while (isNeedRepeat);

            return exception;
        }

        public async Task<Exception> TryGetSearchedPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(GetSearchedPosts);
        }

        private async Task<Exception> GetSearchedPosts(CancellationToken ct)
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
                var response = await Api.GetPostsByCategory(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryGetSearchedPosts));
            } while (isNeedRepeat);

            return exception;
        }
    }
}
