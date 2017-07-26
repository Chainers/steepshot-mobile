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

		public async Task<OperationResult<LogoutResponse>> Logout()
		{
			var request = new LogoutRequest(UserPrincipal.Instance.CurrentUser.SessionId);
			return await Api.Logout(request);
		}

		public async Task<OperationResult<SetNsfwResponse>> SetNsfw(bool value)
		{
			return await Api.SetNsfw(new SetNsfwRequest(UserPrincipal.Instance.Cookie, value));
		}

		public async Task<OperationResult<SetLowRatedResponse>> SetLowRated(bool value)
		{
			return await Api.SetLowRated(new SetLowRatedRequest(UserPrincipal.Instance.Cookie, value));
		}

		public async Task<OperationResult<IsLowRatedResponse>> IsLowRated()
		{
			return await Api.IsLowRated(new IsLowRatedRequest(UserPrincipal.Instance.Cookie));
		}

		public async Task<OperationResult<IsNsfwResponse>> IsNsfw()
		{
			return await Api.IsNsfw(new IsNsfwRequest(UserPrincipal.Instance.Cookie));
		}
	}
}
