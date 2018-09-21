using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class TransferPresenter : PreSignInPresenter
    {
        public async Task<OperationResult<VoidResponse>> TryTransferAsync(UserInfo userInfo, string recipient, string amount, CurrencyType type, string memo = null)
        {
            var transferModel = new TransferModel(userInfo, recipient, amount, type);

            if (!string.IsNullOrEmpty(memo))
                transferModel.Memo = memo;

            return await TryRunTaskAsync<TransferModel, VoidResponse>(TransferAsync, OnDisposeCts.Token, transferModel).ConfigureAwait(false);
        }

        private Task<OperationResult<VoidResponse>> TransferAsync(TransferModel model, CancellationToken ct)
        {
            return Api.TransferAsync(model, ct);
        }

        public async Task<OperationResult<VoidResponse>> TryPowerUpOrDownAsync(BalanceModel balance, PowerAction powerAction)
        {
            var model = new PowerUpDownModel(balance, powerAction);
            return await TryRunTaskAsync<PowerUpDownModel, VoidResponse>(PowerDownOrDownAsync, OnDisposeCts.Token, model).ConfigureAwait(false);
        }

        private Task<OperationResult<VoidResponse>> PowerDownOrDownAsync(PowerUpDownModel model, CancellationToken ct)
        {
            return Api.PowerUpOrDownAsync(model, ct);
        }
    }
}
