using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PreSignInPresenter : BasePresenter
    {
        public async Task<OperationResult<UserProfileResponse>> TryGetAccountInfo(string login)
        {
            return await TryRunTask<string, UserProfileResponse>(GetAccountInfo, OnDisposeCts.Token, login);
        }

        private Task<OperationResult<UserProfileResponse>> GetAccountInfo(string login, CancellationToken ct)
        {
            var req = new UserProfileModel(login);
            return Api.GetUserProfile(req, ct);
        }
    }
}
