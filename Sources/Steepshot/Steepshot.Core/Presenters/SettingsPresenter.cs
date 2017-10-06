using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class SettingsPresenter : BasePresenter
    {
        public async Task<OperationResult<UserProfileResponse>> TryGetUserInfo()
        {
            return await TryRunTask(GetUserInfo, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None));
        }

        private Task<OperationResult<UserProfileResponse>> GetUserInfo(CancellationTokenSource cts)
        {
            var req = new UserProfileRequest(User.Login)
            {
                Login = User.Login
            };
            return Api.GetUserProfile(req);
        }
    }
}
