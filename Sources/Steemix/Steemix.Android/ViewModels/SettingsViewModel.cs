using System.Threading.Tasks;
using Steemix.Library.Models.Requests;

namespace Steemix.Droid.ViewModels
{
    public class SettingsViewModel : MvvmViewModelBase
    {
        public async Task<UserInfoResponse> GetUserInfo()
        {
            var req = new UserInfoRequest(UserPrincipal.CurrentUser.Token, UserPrincipal.CurrentUser.Login);
            var response = await Manager.GetUserInfo(req);
            return response;
        }
    }
}
