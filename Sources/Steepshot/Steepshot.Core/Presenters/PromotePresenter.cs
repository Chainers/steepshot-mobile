using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PromotePresenter : TransferPresenter
    {
        private readonly SteepshotApiClient _steepshotApiClient;

        public PromotePresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient)
            : base(connectionService, logService, ditchClient)
        {
            _steepshotApiClient = steepshotApiClient;
        }

        public async Task<OperationResult<PromoteResponse>> FindPromoteBotAsync(PromoteRequest request)
        {
            return await _steepshotApiClient.FindPromoteBotAsync(request, OnDisposeCts.Token).ConfigureAwait(false);
        }
    }
}
