using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class SignInPresenter : BasePresenter
	{
		public SignInPresenter(SignInView view):base(view)
		{
			
		}

		public async Task<OperationResult<LoginResponse>> SignIn(string login, string postingKey)
		{
			var request = new LoginWithPostingKeyRequest(login, postingKey);
			var response = await Api.LoginWithPostingKey(request);
			return response;
		}
	}
}
