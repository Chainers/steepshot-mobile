using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PostDescriptionPresenter : BasePresenter
    {
        public async Task<OperationResult<ImageUploadResponse>> TryUpload(UploadImageRequest request)
        {
            return await TryRunTask(Upload, CancellationToken.None, request);
        }

        private async Task<OperationResult<ImageUploadResponse>> Upload(CancellationToken ct, UploadImageRequest request)
        {
            return await Api.Upload(request, ct);
        }
    }
}
