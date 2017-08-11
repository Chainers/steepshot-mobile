using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Presenter
{
	public class SettingsPresenter : BasePresenter
	{
		public SettingsPresenter(IBaseView view):base(view)
		{
		}

		public async Task<OperationResult<UserProfileResponse>> GetUserInfo()
		{
		    var req = new UserProfileRequest(User.Login)
		    {
		        Login = User.CurrentUser.Login,
		        SessionId = User.CurrentUser.SessionId
		    };

			var response = await Api.GetUserProfile(req);
			return response;
		}

		public async Task<OperationResult<LogoutResponse>> Logout()
		{
			var request = new LogoutRequest(User.CurrentUser.SessionId)
			{
			    Login = User.CurrentUser.Login,
			    SessionId = User.CurrentUser.SessionId
			};
            return await Api.Logout(request);
		}
	}
}
