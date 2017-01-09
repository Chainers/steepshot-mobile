using System;
using System.Threading.Tasks;
using Steemix.Library.Models.Requests;
using Steemix.Library.Models.Responses;

namespace Steemix.Android
{
	public class SignUpViewModel : MvvmViewModelBase
	{
		public async Task<RegisterResponse> SignUp(string login, string password, string postingkey)
		{
		    var request = new RegisterRequest(postingkey, login, password);
            var response = await Manager.Register(request);
		    return response;
		}
	}
}
