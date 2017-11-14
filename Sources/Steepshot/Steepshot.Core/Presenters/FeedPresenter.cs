using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public sealed class FeedPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 20;

        public async Task<List<string>> TryLoadNextTopPosts()
        {
            if (IsLastReaded)
                return null;

            return await RunAsSingleTask(LoadNextTopPosts);
        }

        private async Task<List<string>> LoadNextTopPosts(CancellationToken ct)
        {
            var request = new CensoredNamedRequestWithOffsetLimitFields
            {
                Login = User.Login,
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            List<string> errors;
            OperationResult<UserPostResponse> response;
            bool isNeedRepeat;
            do
            {
                response = await Api.GetUserRecentPosts(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out errors);
            } while (isNeedRepeat);

            return errors;
        }
    }
}
