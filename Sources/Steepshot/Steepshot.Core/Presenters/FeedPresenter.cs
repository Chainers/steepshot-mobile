using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public sealed class FeedPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 20;

        public async Task<Exception> TryLoadNextTopPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<Exception> LoadNextTopPosts(CancellationToken ct)
        {
            var request = new CensoredNamedRequestWithOffsetLimitModel
            {
                Login = AppSettings.User.Login,
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetUserRecentPosts(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextTopPosts));
            } while (isNeedRepeat);

            return exception;
        }
    }
}
