using System;
using System.Threading.Tasks;
using Steemix.Library.Models.Requests;

namespace Steemix.Android
{
	public class SignUpViewModel : MvvmViewModelBase
	{
		public SignUpViewModel()
		{
		}

		public async Task<bool> SignUp(string login, string password, string postingkey)
		{
		    var request = new RegisterRequest(postingkey, login, password);
		    if (IsValid(request))
		    {
                var response = await Manager.Register(request);
		        if (string.IsNullOrEmpty(response.error))
		        {
		            UserPrincipal.CreatePrincipal(response);
		            return true;
		        }
		    }

		    return false;
		}

	    private bool IsValid(RegisterRequest request)
		{
			if (string.IsNullOrEmpty(request.username)
				|| string.IsNullOrEmpty(request.password)
				|| string.IsNullOrEmpty(request.posting_key))
				return false;
			return true;
		}
	}
}
