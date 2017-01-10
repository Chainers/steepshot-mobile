using System.Threading.Tasks;
using Steemix.Library.Models.Requests;
using Steemix.Library.Models.Responses;

namespace Steemix.Droid
{
    public class SignInViewModel : MvvmViewModelBase
    {
        public async Task<LoginResponse> SignIn(string login, string password)
        {
            var request = new LoginRequest(login, password);
            var response = await Manager.Login(request);
            return response;
        }
    }
}
