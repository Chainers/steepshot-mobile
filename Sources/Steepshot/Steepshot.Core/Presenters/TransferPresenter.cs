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
        public async Task<OperationResult<VoidResponse>> TryTransfer(string recipient, double amount, CurrencyType type, string memo = null)
        {
            return await TryRunTask<Tuple<string, double, CurrencyType, string>, VoidResponse>(Transfer, OnDisposeCts.Token, new Tuple<string, double, CurrencyType, string>(recipient, amount, type, memo));
        }

        private Task<OperationResult<VoidResponse>> Transfer(Tuple<string, double, CurrencyType, string> transferData, CancellationToken ct)
        {
            var transferModel = new TransferModel(
                AppSettings.User.Login,
                AppSettings.User.ActiveKey,
                transferData.Item1,
                (long)(transferData.Item3 == CurrencyType.Sbd ? transferData.Item2 * 10000000 : transferData.Item2 * 1000),
                (byte)(transferData.Item3 == CurrencyType.Sbd ? 6 : 3),
                transferData.Item3);

            if (string.IsNullOrEmpty(transferData.Item4))
                transferModel.Memo = transferData.Item4;

            return Api.Transfer(transferModel, ct);
        }
    }
}
