using System;
using System.Threading.Tasks;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Extensions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Presenters
{
    public sealed class PreSearchPresenter : BasePostPresenter
    {
        public PostType PostType = PostType.Hot;
        private const int ItemsLimit = 18;
        public string Tag;

        public PreSearchPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, User user, SteepshotClient steepshotClient)
            : base(connectionService, logService, ditchClient, steepshotApiClient, user, steepshotClient)
        {
        }

        public async Task<Exception> TryLoadNextTopPostsAsync()
        {
            if (IsLastReaded)
                return null;

            var request = new PostsModel(PostType)
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await RunAsSingleTaskAsync(SteepshotApiClient.GetPostsAsync, request).ConfigureAwait(true);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextTopPostsAsync));
            } while (isNeedRepeat);

            return exception;
        }


        public async Task<Exception> TryGetSearchedPostsAsync()
        {
            if (IsLastReaded)
                return null;

            var request = new PostsByCategoryModel(PostType, Tag.TagToEn())
            {
                Login = User.Login,
                Limit = string.IsNullOrEmpty(OffsetUrl) ? ItemsLimit : ItemsLimit + 1,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await RunAsSingleTaskAsync(SteepshotApiClient.GetPostsByCategoryAsync, request).ConfigureAwait(true);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryGetSearchedPostsAsync));
            } while (isNeedRepeat);

            return exception;
        }
    }
}
