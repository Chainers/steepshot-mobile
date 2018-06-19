using System;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class TransferPresenter : BasePresenter
    {
        public async Task<OperationResult<VoidResponse>> TryTransfer(string login, string postingKey)
        {
            return await TryRunTask<string, string, VoidResponse>(Transfer, OnDisposeCts.Token, login, postingKey);
        }

        private Task<OperationResult<VoidResponse>> Transfer(string login, string postingKey, CancellationToken ct)
        {
            var request = new AuthorizedActiveModel(login, postingKey);

            var t = new TransferModel("", "", "", 8768, 7, CurrencyType.Steem);

            return Api.Transfer(t, ct);
        }
    }
}
