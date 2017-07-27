using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.View;

namespace Steepshot.Presenter
{
	public class SettingsPresenter : BasePresenter
	{
		public SettingsPresenter(SettingsView view):base(view)
		{
		}

		public async Task<OperationResult<UserProfileResponse>> GetUserInfo()
		{
			var req = new UserProfileRequest(User.Login, User.CurrentUser);
			var response = await Api.GetUserProfile(req);
			return response;
		}

		public async Task<OperationResult<LogoutResponse>> Logout()
		{
			var request = new LogoutRequest(User.CurrentUser);
			return await Api.Logout(request);
		}
	}
}
