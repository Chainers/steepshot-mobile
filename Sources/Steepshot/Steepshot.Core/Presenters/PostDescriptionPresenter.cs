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
        public async Task<OperationResult<MediaModel>> TryUploadMedia(UploadMediaModel model)
        {
            return await TryRunTask<UploadMediaModel, MediaModel>(UploadMedia, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<MediaModel>> UploadMedia(UploadMediaModel model, CancellationToken ct)
        {
            return await Api.UploadMedia(model, ct);
        }

        public async Task<OperationResult<VoidResponse>> TryCreateOrEditPost(PreparePostModel model)
        {
            return await TryRunTask<PreparePostModel, VoidResponse>(CreateOrEditPost, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<VoidResponse>> CreateOrEditPost(PreparePostModel model, CancellationToken ct)
        {
            return await Api.CreateOrEditPost(model, ct);
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
