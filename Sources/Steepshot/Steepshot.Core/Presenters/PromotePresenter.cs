using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PromotePresenter : TransferPresenter
    {
        public async Task<OperationResult<PromoteResponse>> FindPromoteBotAsync(PromoteRequest request)
        {
            return await Api.FindPromoteBotAsync(request).ConfigureAwait(false);
        }
    }
}
