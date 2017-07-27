using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.View;

namespace Steepshot.Presenter
{
    public class SignInPresenter : BasePresenter
    {
        public SignInPresenter(ISignInView view) : base(view) { }

        public Task<OperationResult<LoginResponse>> SignIn(string login, string postingKey)
        {
            var request = new LoginWithPostingKeyRequest(login, postingKey);
            return Api.LoginWithPostingKey(request);
        }
    }
}
