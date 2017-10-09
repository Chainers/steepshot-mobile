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
            return await TryRunTask(GetAccountInfo, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), login);
        }

        private Task<OperationResult<UserProfileResponse>> GetAccountInfo(CancellationTokenSource cts, string login)
        {
            var req = new UserProfileRequest(login);
            return Api.GetUserProfile(req, cts);
        }
    }
}
