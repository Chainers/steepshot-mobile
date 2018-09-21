using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PreSignInPresenter : BasePresenter
    {
        public async Task<OperationResult<AccountInfoResponse>> TryGetAccountInfoAsync(string login)
        {
            return await TryRunTaskAsync<string, AccountInfoResponse>(GetAccountInfoAsync, OnDisposeCts.Token, login).ConfigureAwait(false);
        }

        protected Task<OperationResult<AccountInfoResponse>> GetAccountInfoAsync(string login, CancellationToken ct)
        {
            return Api.GetAccountInfoAsync(login, ct);
        }
    }
}
