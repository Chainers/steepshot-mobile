using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class ChangePasswordPresenter : BasePresenter
	{
		public ChangePasswordPresenter(ChangePasswordView view) : base(view)
		{
		}

		public async Task<OperationResult<ChangePasswordResponse>> ChangePassword(string oldPassword, string newPassword)
		{
			var req = new ChangePasswordRequest(UserPrincipal.Instance.CurrentUser.SessionId, oldPassword, newPassword);
			var response = await Api.ChangePassword(req);
			return response;
		}
	}
}
