using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public sealed class PreSignInPresenter : BasePresenter
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

    public sealed class LolPresenter : ListPresenter<string>
    {
        public async Task<ErrorBase> TryGetAccountInfo(string login)
        {
            return await RunAsSingleTask(GetAccountInfo, login);
        }

        private async Task<ErrorBase> GetAccountInfo(string login, CancellationToken ct)
        {
            var req = new UserProfileModel(login);
            var response = await Api.GetUserProfile(req, ct);
            return response.Error;
        }
    }
}
