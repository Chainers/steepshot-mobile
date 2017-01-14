using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
	public class SignUpViewModel : MvvmViewModelBase
	{
		public async Task<OperationResult<LoginResponse>> SignUp(string login, string password, string postingkey)
		{
		    var request = new RegisterRequest(postingkey, login, password);
            var response = await Manager.Register(request);
		    return response;
		}
	}
}
