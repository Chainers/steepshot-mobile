using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class SettingsPresenter : BasePresenter
	{
		public SettingsPresenter(SettingsView view):base(view)
		{
		}

		public async Task<OperationResult<UserProfileResponse>> GetUserInfo()
		{
			var req = new UserProfileRequest(UserPrincipal.Instance.CurrentUser.Login) {SessionId = UserPrincipal.Instance.Cookie};
			var response = await Api.GetUserProfile(req);
			return response;
		}
	}
}
