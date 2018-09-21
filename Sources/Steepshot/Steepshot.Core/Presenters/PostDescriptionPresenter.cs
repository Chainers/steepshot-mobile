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
        public async Task<OperationResult<UUIDModel>> TryUploadMediaAsync(UploadMediaModel model)
        {
            return await TryRunTaskAsync<UploadMediaModel, UUIDModel>(UploadMediaAsync, OnDisposeCts.Token, model).ConfigureAwait(false);
        }

        private async Task<OperationResult<UUIDModel>> UploadMediaAsync(UploadMediaModel model, CancellationToken ct)
        {
            return await Api.UploadMediaAsync(model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<UploadMediaStatusModel>> TryGetMediaStatusAsync(UUIDModel uuid)
        {
            return await TryRunTaskAsync<UUIDModel, UploadMediaStatusModel>(GetMediaStatusAsync, OnDisposeCts.Token, uuid).ConfigureAwait(false);
        }

        private async Task<OperationResult<UploadMediaStatusModel>> GetMediaStatusAsync(UUIDModel uuid, CancellationToken ct)
        {
            return await Api.GetMediaStatusAsync(uuid, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<MediaModel>> TryGetMediaResultAsync(UUIDModel uuid)
        {
            return await TryRunTaskAsync<UUIDModel, MediaModel>(GetMediaResultAsync, OnDisposeCts.Token, uuid).ConfigureAwait(false);
        }

        private async Task<OperationResult<MediaModel>> GetMediaResultAsync(UUIDModel uuid, CancellationToken ct)
        {
            return await Api.GetMediaResultAsync(uuid, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<PreparePostResponse>> TryCheckForPlagiarismAsync(PreparePostModel model)
        {
            return await TryRunTaskAsync<PreparePostModel, PreparePostResponse>(CheckForPlagiarismAsync, OnDisposeCts.Token, model).ConfigureAwait(false);
        }

        private async Task<OperationResult<PreparePostResponse>> CheckForPlagiarismAsync(PreparePostModel model, CancellationToken ct)
        {
            return await Api.CheckPostForPlagiarismAsync(model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> TryCreateOrEditPostAsync(PreparePostModel model)
        {
            return await TryRunTaskAsync<PreparePostModel, VoidResponse>(CreateOrEditPostAsync, OnDisposeCts.Token, model).ConfigureAwait(false);
        }

        private async Task<OperationResult<VoidResponse>> CreateOrEditPostAsync(PreparePostModel model, CancellationToken ct)
        {
            return await Api.CreateOrEditPostAsync(model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<SpamResponse>> TryCheckForSpamAsync(string username)
        {
            return await TryRunTaskAsync<string, SpamResponse>(CheckForSpamAsync, OnDisposeCts.Token, username).ConfigureAwait(false);
        }

        private async Task<OperationResult<SpamResponse>> CheckForSpamAsync(string username, CancellationToken token)
        {
            return await Api.CheckForSpamAsync(username, token).ConfigureAwait(false);
        }
    }
}
