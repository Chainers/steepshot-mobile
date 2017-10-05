using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class TermOfServicePresenter : BasePresenter
    {
        public async Task<OperationResult<TermOfServiceResponse>> TryGetTermsOfService()
        {
            return await TryRunTask(TermsOfService);
        }

        private async Task<OperationResult<TermOfServiceResponse>> TermsOfService()
        {
            return await Api.TermsOfService();
        }
    }
}