using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class PreSignInPresenter : BasePresenter
    {
        protected readonly BaseDitchClient DitchClient;

        public PreSignInPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient)
            : base(connectionService, logService)
        {
            DitchClient = ditchClient;
        }

        public async Task<OperationResult<AccountInfoResponse>> TryGetAccountInfoAsync(string login)
        {
            return await TaskHelper.TryRunTaskAsync(DitchClient.GetAccountInfoAsync, login, OnDisposeCts.Token).ConfigureAwait(false);
        }
    }
}
