using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Models.Common;
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
    }
}
