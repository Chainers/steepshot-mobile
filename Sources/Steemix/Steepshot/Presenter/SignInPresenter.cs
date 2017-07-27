using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
    public class SignInPresenter : BasePresenter
    {
        public SignInPresenter(SignInView view) : base(view) { }

        public Task<OperationResult<LoginResponse>> SignIn(string login, string postingKey)
        {
            var request = new LoginWithPostingKeyRequest(login, postingKey);
            return Api.LoginWithPostingKey(request);
        }
    }
}
