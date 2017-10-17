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
            return await TryRunTask<string, UserProfileResponse>(GetAccountInfo, CancellationToken.None, login);
        }

        private Task<OperationResult<UserProfileResponse>> GetAccountInfo(CancellationToken ct, string login)
        {
            var req = new UserProfileRequest(login);
            return Api.GetUserProfile(req, ct);
        }
    }
}
