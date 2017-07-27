using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
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
