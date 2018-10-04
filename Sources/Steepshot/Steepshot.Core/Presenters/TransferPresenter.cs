using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class TransferPresenter : PreSignInPresenter
    {
        public TransferPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient)
            : base(connectionService, logService, ditchClient)
        {
        }

        public async Task<OperationResult<VoidResponse>> TryTransferAsync(UserInfo userInfo, string recipient, string amount, CurrencyType type, string memo = null)
        {
            var transferModel = new TransferModel(userInfo, recipient, amount, type);

            if (!string.IsNullOrEmpty(memo))
                transferModel.Memo = memo;

            return await TaskHelper.TryRunTaskAsync(DitchClient.TransferAsync, transferModel, OnDisposeCts.Token).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> TryPowerUpOrDownAsync(BalanceModel balance, PowerAction powerAction)
        {
            var model = new PowerUpDownModel(balance, powerAction);
            return await TaskHelper.TryRunTaskAsync(DitchClient.PowerUpOrDownAsync, model, OnDisposeCts.Token).ConfigureAwait(false);
        }
    }
}
