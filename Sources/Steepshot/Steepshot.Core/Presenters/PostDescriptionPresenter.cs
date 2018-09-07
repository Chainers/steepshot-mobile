using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Ditch.Core.JsonRpc;

namespace Steepshot.Core.Presenters
{
    public sealed class PostDescriptionPresenter : BasePresenter
    {
        public async Task<OperationResult<UUIDModel>> TryUploadMedia(UploadMediaModel model)
        {
            return await TryRunTask<UploadMediaModel, UUIDModel>(UploadMedia, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<UUIDModel>> UploadMedia(UploadMediaModel model, CancellationToken ct)
        {
            return await Api.UploadMedia(model, ct);
        }

        public async Task<OperationResult<UploadMediaStatusModel>> TryGetMediaStatus(UUIDModel uuid)
        {
            return await TryRunTask<UUIDModel, UploadMediaStatusModel>(GetMediaStatus, OnDisposeCts.Token, uuid);
        }

        private async Task<OperationResult<UploadMediaStatusModel>> GetMediaStatus(UUIDModel uuid, CancellationToken ct)
        {
            return await Api.GetMediaStatus(uuid, ct);
        }

        public async Task<OperationResult<MediaModel>> TryGetMediaResult(UUIDModel uuid)
        {
            return await TryRunTask<UUIDModel, MediaModel>(GetMediaResult, OnDisposeCts.Token, uuid);
        }

        private async Task<OperationResult<MediaModel>> GetMediaResult(UUIDModel uuid, CancellationToken ct)
        {
            return await Api.GetMediaResult(uuid, ct);
        }

        public async Task<OperationResult<PreparePostResponse>> TryCheckForPlagiarism(PreparePostModel model)
        {
            return await TryRunTask<PreparePostModel, PreparePostResponse>(CheckForPlagiarism, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<PreparePostResponse>> CheckForPlagiarism(PreparePostModel model, CancellationToken ct)
        {
            return await Api.CheckPostForPlagiarism(model, ct);
        }

        public async Task<OperationResult<VoidResponse>> TryCreateOrEditPost(PreparePostModel model)
        {
            return await TryRunTask<PreparePostModel, VoidResponse>(CreateOrEditPost, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<VoidResponse>> CreateOrEditPost(PreparePostModel model, CancellationToken ct)
        {
            return await Api.CreateOrEditPost(model, ct);
        }

        public async Task<OperationResult<SpamResponse>> TryCheckForSpam(string username)
        {
            return await TryRunTask<string, SpamResponse>(CheckForSpam, OnDisposeCts.Token, username);
        }

        private async Task<OperationResult<SpamResponse>> CheckForSpam(string username, CancellationToken token)
        {
            return await Api.CheckForSpam(username, token);
        }
    }
}
