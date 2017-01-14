using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
    public class SignInViewModel : MvvmViewModelBase
    {
        public async Task<OperationResult<LoginResponse>> SignIn(string login, string password)
        {
            var request = new LoginRequest(login, password);
            var response = await Manager.Login(request);
            return response;
        }
    }
}
