using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class SignUpPresenter : BasePresenter
	{
		public SignUpPresenter(SignUpView view):base(view)
		{
		}

		public async Task<OperationResult<LoginResponse>> SignUp(string login, string password, string postingkey)
		{
			var request = new RegisterRequest(postingkey, login, password);
			var response = await Api.Register(request);
			return response;
		}
	}
}
