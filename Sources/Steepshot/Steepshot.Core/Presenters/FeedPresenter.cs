using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class FeedPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 20;

        public async Task<ErrorBase> TryLoadNextTopPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<ErrorBase> LoadNextTopPosts(CancellationToken ct)
        {
            var request = new CensoredNamedRequestWithOffsetLimitModel
            {
                Login = User.Login,
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            ErrorBase error;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetUserRecentPosts(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out error, nameof(TryLoadNextTopPosts));
            } while (isNeedRepeat);

            return error;
        }
    }
}
