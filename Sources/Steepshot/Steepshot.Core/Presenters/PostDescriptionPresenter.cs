using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using System.IO;

namespace Steepshot.Core.Presenters
{
    public class PostDescriptionPresenter : TagsPresenter
    {
        public async Task<OperationResult<UploadResponse>> TryUploadWithPrepare(UploadImageModel model)
        {
            return await TryRunTask<UploadImageModel, UploadResponse>(UploadWithPrepare, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageModel model, CancellationToken ct)
        {
            return await Api.UploadWithPrepare(model, ct);
        }


        public async Task<OperationResult<VoidResponse>> TryCreatePost(UploadImageModel model, UploadResponse uploadResponse)
        {
            return await TryRunTask<UploadImageModel, UploadResponse, VoidResponse>(CreatePost, OnDisposeCts.Token, model, uploadResponse);
        }

        private async Task<OperationResult<VoidResponse>> CreatePost(UploadImageModel model, UploadResponse uploadResponse, CancellationToken ct)
        {
            return await Api.CreatePost(model, uploadResponse, ct);
        }

        public async Task<OperationResult<NsfwRate>> TryNsfwCheck(Stream stream)
        {
            return await TryRunTask<Stream, NsfwRate>(NsfwCheck, OnDisposeCts.Token, stream);
        }

        private async Task<OperationResult<NsfwRate>> NsfwCheck(Stream stream, CancellationToken token)
        {
            return await Api.NsfwCheck(stream, token);
        }
    }
}
