using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
    public class SettingsViewModel : MvvmViewModelBase
    {
        public async Task<OperationResult<UserResponse>> GetUserInfo()
        {
            var req = new UserRequest(UserPrincipal.CurrentUser.SessionId, UserPrincipal.CurrentUser.Login);
            var response = await Manager.GetUserProfile(req);
            return response;
        }
    }
}
