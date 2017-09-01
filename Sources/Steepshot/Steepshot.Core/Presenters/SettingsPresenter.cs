using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class SettingsPresenter : BasePresenter
    {
        public async Task<OperationResult<UserProfileResponse>> GetUserInfo()
        {
            var req = new UserProfileRequest(User.Login)
            {
                Login = User.Login
            };

            var response = await Api.GetUserProfile(req);
            return response;
        }

        public async Task<OperationResult<LogoutResponse>> Logout()
        {
            var request = new AuthorizedRequest(User.UserInfo);
            return await Api.Logout(request);
        }
    }
}