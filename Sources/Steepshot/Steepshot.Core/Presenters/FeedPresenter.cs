using System;
using System.Threading.Tasks;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class FeedPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 20;

        public FeedPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, User user, SteepshotClient steepshotClient)
            : base(connectionService, logService, ditchClient, steepshotApiClient, user, steepshotClient)
        {
        }

        public async Task<Exception> TryLoadNextTopPostsAsync()
        {
            if (IsLastReaded)
                return null;

            var request = new CensoredNamedRequestWithOffsetLimitModel
            {
                Login = User.Login,
                Limit = ItemsLimit,
                Offset = OffsetUrl,
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated
            };

            Exception exception;
            bool isNeedRepeat;
            do
            {
                var response = await RunAsSingleTaskAsync(SteepshotApiClient.GetUserRecentPostsAsync, request).ConfigureAwait(false);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextTopPostsAsync));
            } while (isNeedRepeat);

            return exception;
        }
    }
}
