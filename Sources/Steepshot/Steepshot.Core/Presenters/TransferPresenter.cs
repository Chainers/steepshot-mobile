using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class TransferPresenter : BasePresenter
    {
        public async Task<OperationResult<VoidResponse>> TryTransfer(string recipient, string amount, CurrencyType type, string memo = null)
        {
            var transferModel = new TransferModel(AppSettings.User.UserInfo, recipient, amount, type);

            if (!string.IsNullOrEmpty(memo))
                transferModel.Memo = memo;

            return await TryRunTask<TransferModel, VoidResponse>(Transfer, OnDisposeCts.Token, transferModel);
        }

        private Task<OperationResult<VoidResponse>> Transfer(TransferModel model, CancellationToken ct)
        {
            return Api.Transfer(model, ct);
        }

        public async Task<OperationResult<VoidResponse>> TryPowerUpOrDown(BalanceModel balance, PowerAction powerAction)
        {
            var value = powerAction == PowerAction.PowerUp
                ? balance.Value.ToString(CultureInfo.InvariantCulture)
                : (balance.Value / AppSettings.User.AccountInfo.SteemPerVestsRatio).ToString("F", CultureInfo.InvariantCulture);
            var model = new PowerUpDownModel(balance.UserInfo, balance.UserInfo.Login, balance.UserInfo.Login, value, balance.CurrencyType, powerAction);
            return await TryRunTask<PowerUpDownModel, VoidResponse>(PowerDownOrDown, OnDisposeCts.Token, model);
        }

        private Task<OperationResult<VoidResponse>> PowerDownOrDown(PowerUpDownModel model, CancellationToken ct)
        {
            return Api.PowerUpOrDown(model, ct);
        }
    }
}
