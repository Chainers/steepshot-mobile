using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PreSignInPresenter : BasePresenter
    {
        public async Task<OperationResult<AccountInfoResponse>> TryGetAccountInfo(string login)
        {
            return await TryRunTask<string, AccountInfoResponse>(GetAccountInfo, OnDisposeCts.Token, login);
        }

        protected Task<OperationResult<AccountInfoResponse>> GetAccountInfo(string login, CancellationToken ct)
        {
            return Api.GetAccountInfo(login, ct);
        }
    }
}
