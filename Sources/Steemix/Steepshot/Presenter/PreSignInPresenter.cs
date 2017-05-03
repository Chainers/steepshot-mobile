using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class PreSignInPresenter : BasePresenter
	{
		public PreSignInPresenter(PreSignInView view) : base(view)
		{
		}

		public async Task<OperationResult<UserProfileResponse>> GetAccountInfo(string login)
		{
			var req = new UserProfileRequest(login) { };
			var response = await Api.GetUserProfile(req);
			return response;
		}
	}
}
