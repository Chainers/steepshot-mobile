using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class TermOfServicePresenter : BasePresenter
    {
        public async Task<OperationResult<TermOfServiceResponse>> TryGetTermsOfService()
        {
            return await TryRunTask<TermOfServiceResponse>(TermsOfService, OnDisposeCts.Token);
        }

        private async Task<OperationResult<TermOfServiceResponse>> TermsOfService(CancellationToken ct)
        {
            return await Api.TermsOfService(ct);
        }
    }
}
