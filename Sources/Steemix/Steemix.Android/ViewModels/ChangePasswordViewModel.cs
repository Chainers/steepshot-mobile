using System.Threading.Tasks;
using Steemix.Library.Models.Requests;

namespace Steemix.Droid.ViewModels
{
    public class ChangePasswordViewModel : MvvmViewModelBase
    {
        public async Task<ChangePasswordResponse> ChangePassword(string oldPassword, string newPassword)
        {
            var req = new ChangePasswordRequest(UserPrincipal.CurrentUser.Token, oldPassword, newPassword);
            var response = await Manager.ChangePassword(req);
            return response;
        }
    }
}
