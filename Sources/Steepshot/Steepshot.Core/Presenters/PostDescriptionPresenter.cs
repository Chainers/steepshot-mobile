using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class PostDescriptionPresenter : TagsPresenter
    {
        public async Task<OperationResult<UploadResponse>> TryUploadWithPrepare(UploadImageModel model)
        {
            return await TryRunTask<UploadImageModel, UploadResponse>(UploadWithPrepare, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<UploadResponse>> UploadWithPrepare(CancellationToken ct, UploadImageModel model)
        {
            return await Api.UploadWithPrepare(model, ct);
        }


        public async Task<OperationResult<ImageUploadResponse>> TryCreatePost(UploadImageModel model, UploadResponse uploadResponse)
        {
            return await TryRunTask<UploadImageModel, UploadResponse, ImageUploadResponse>(CreatePost, OnDisposeCts.Token, model, uploadResponse);
        }

        private async Task<OperationResult<ImageUploadResponse>> CreatePost(CancellationToken ct, UploadImageModel model, UploadResponse uploadResponse)
        {
            return await Api.CreatePost(model, uploadResponse, ct);
        }
    }
}
