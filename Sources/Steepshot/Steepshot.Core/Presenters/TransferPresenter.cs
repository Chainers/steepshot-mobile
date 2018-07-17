using System;
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
        public async Task<OperationResult<VoidResponse>> TryTransfer(string recipient, double amount, CurrencyType type, string chainCurrency, string memo = null)
        {
            return await TryRunTask<(string Recipient, double Amount, CurrencyType Type, string ChainCurrency, string Memo), VoidResponse>(Transfer, OnDisposeCts.Token, (recipient, amount, type, chainCurrency, memo));
        }

        private Task<OperationResult<VoidResponse>> Transfer((string Recipient, double Amount, CurrencyType Type, string ChainCurrency, string Memo) transferData, CancellationToken ct)
        {
            var transferModel = new TransferModel(
                AppSettings.User.Login,
                AppSettings.User.ActiveKey,
                transferData.Recipient,
                (long)(transferData.Amount * 1000),
                3,
                transferData.Type,
                transferData.ChainCurrency);

            if (!string.IsNullOrEmpty(transferData.Memo))
                transferModel.Memo = transferData.Memo;

            return Api.Transfer(transferModel, ct);
        }
    }
}
