using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PostDescriptionPresenter : BasePresenter
    {
        public new bool CheckInternetConnection()
        {
            return base.CheckInternetConnection();
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request)
        {
            return await Api.Upload(request);
        }
    }
}
