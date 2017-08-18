using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class TermsPresenter : BasePresenter
    {
        public async Task<OperationResult<TermOfServiceResponse>> GetTermsOfService()
        {
            return await Api.TermsOfService();
        }
    }
}