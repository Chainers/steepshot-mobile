using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class SignInPresenter : BasePresenter
    {
        public Task<OperationResult<LoginResponse>> SignIn(string login, string postingKey)
        {
            var request = new AuthorizedRequest(login, postingKey);
            return Api.LoginWithPostingKey(request);
        }
    }
}