using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.View;

namespace Steepshot.Presenter
{
    public class PreSignInPresenter : BasePresenter
    {
        public PreSignInPresenter(PreSignInView view) : base(view)
        {
        }

        public async Task<OperationResult<UserProfileResponse>> GetAccountInfo(string login)
        {
            var req = new UserProfileRequest(login);
            var response = await Api.GetUserProfile(req);
            return response;
        }
    }
}
