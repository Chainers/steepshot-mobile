using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PostDescriptionPresenter : TagsPresenter
    {
        public async Task<OperationResult<UploadResponse>> TryUploadWithPrepare(UploadImageRequest request)
        {
            return await TryRunTask<UploadImageRequest, UploadResponse>(UploadWithPrepare, CancellationToken.None, request);
        }

        private async Task<OperationResult<UploadResponse>> UploadWithPrepare(CancellationToken ct, UploadImageRequest request)
        {
            return await Api.UploadWithPrepare(request, ct);
        }


        public async Task<OperationResult<ImageUploadResponse>> TryUpload(UploadImageRequest request, UploadResponse uploadResponse)
        {
            return await TryRunTask<UploadImageRequest, UploadResponse, ImageUploadResponse>(Upload, CancellationToken.None, request, uploadResponse);
        }

        private async Task<OperationResult<ImageUploadResponse>> Upload(CancellationToken ct, UploadImageRequest request, UploadResponse uploadResponse)
        {
            return await Api.Upload(request, uploadResponse, ct);
        }
    }
}
