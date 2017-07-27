using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.View;

namespace Steepshot.Presenter
{
	public class TermsPresenter : BasePresenter
	{
		public TermsPresenter(TermsView view): base(view)
		{
		}

		public async Task<OperationResult<TermOfServiceResponse>> GetTermsOfService()
		{
			return await Api.TermsOfService();
		}
	}
}
