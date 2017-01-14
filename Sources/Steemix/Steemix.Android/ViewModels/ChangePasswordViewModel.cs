using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
    public class ChangePasswordViewModel : MvvmViewModelBase
    {
        public async Task<OperationResult<ChangePasswordResponse>> ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            var req = new ChangePasswordRequest(UserPrincipal.CurrentUser.SessionId, oldPassword, newPassword, confirmNewPassword);
            var response = await ViewModelLocator.Api.ChangePassword(req);
            return response;
        }
    }
}
