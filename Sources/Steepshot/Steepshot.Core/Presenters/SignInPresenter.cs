using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class SignInPresenter : BasePresenter
    {
        public async Task<OperationResult<VoidResponse>> TrySignIn(string login, string postingKey)
        {
            return await TryRunTask<string, string, VoidResponse>(SignIn, OnDisposeCts.Token, login, postingKey);
        }

        private Task<OperationResult<VoidResponse>> SignIn(string login, string postingKey, CancellationToken ct)
        {
            var request = new AuthorizedModel(login, postingKey);
            return Api.LoginWithPostingKey(request, ct);
        }
    }
}