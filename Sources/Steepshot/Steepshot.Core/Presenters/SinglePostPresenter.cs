using System.Threading.Tasks;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class SinglePostPresenter : BasePostPresenter
    {
        public Post PostInfo { get; private set; }

        public SinglePostPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, User user, SteepshotClient steepshotClient)
            : base(connectionService, logService, ditchClient, steepshotApiClient, user, steepshotClient)
        {
        }

        public async Task<OperationResult<Post>> TryLoadPostInfoAsync(string url)
        {
            var request = new NamedInfoModel(url)
            {
                ShowNsfw = User.IsNsfw,
                ShowLowRated = User.IsLowRated,
                Login = User.Login
            };

            var result = await TaskHelper
                .TryRunTaskAsync(SteepshotApiClient.GetPostInfoAsync, request, OnDisposeCts.Token)
                .ConfigureAwait(true);

            if (result.IsSuccess)
            {
                var isAdded = false;
                var item = result.Result;
                if (IsValidMedia(item))
                {
                    CashManager.Add(item);
                    PostInfo = item;
                    isAdded = true;
                }

                NotifySourceChanged(nameof(TryLoadPostInfoAsync), isAdded);
            }
            return result;
        }
    }
}
