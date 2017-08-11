using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Presenter
{
	public class TermsPresenter : BasePresenter
	{
		public TermsPresenter(IBaseView view): base(view)
		{
		}

		public async Task<OperationResult<TermOfServiceResponse>> GetTermsOfService()
		{
			return await Api.TermsOfService();
		}
	}
}
