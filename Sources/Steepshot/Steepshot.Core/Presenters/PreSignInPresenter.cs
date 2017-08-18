using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PreSignInPresenter : BasePresenter
    {
        public async Task<OperationResult<UserProfileResponse>> GetAccountInfo(string login)
        {
            var req = new UserProfileRequest(login);
            var response = await Api.GetUserProfile(req);
            return response;
        }
    }
}
