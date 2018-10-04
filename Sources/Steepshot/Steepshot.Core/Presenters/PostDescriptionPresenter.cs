using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;

namespace Steepshot.Core.Presenters
{
    public sealed class PostDescriptionPresenter : BasePresenter
    {
        private readonly BaseDitchClient _ditchClient;
        private readonly SteepshotApiClient _steepshotApiClient;
        private readonly SteepshotClient _steepshotClient;

        public PostDescriptionPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, SteepshotClient steepshotClient)
            : base(connectionService, logService)
        {
            _ditchClient = ditchClient;
            _steepshotApiClient = steepshotApiClient;
            _steepshotClient = steepshotClient;
        }

        public async Task<OperationResult<UUIDModel>> TryUploadMediaAsync(UploadMediaModel model)
        {
            return await TaskHelper.TryRunTaskAsync(_steepshotClient.UploadMediaAsync, model, OnDisposeCts.Token).ConfigureAwait(false);
        }

        public async Task<OperationResult<UploadMediaStatusModel>> TryGetMediaStatusAsync(UUIDModel uuid)
        {
            return await TaskHelper.TryRunTaskAsync(_steepshotApiClient.GetMediaStatusAsync, uuid, OnDisposeCts.Token).ConfigureAwait(false);
        }

        public async Task<OperationResult<MediaModel>> TryGetMediaResultAsync(UUIDModel uuid)
        {
            return await TaskHelper.TryRunTaskAsync(_steepshotApiClient.GetMediaResultAsync, uuid, OnDisposeCts.Token).ConfigureAwait(false);
        }

        public async Task<OperationResult<PreparePostResponse>> TryCheckForPlagiarismAsync(PreparePostModel model)
        {
            return await TaskHelper.TryRunTaskAsync(_steepshotApiClient.CheckPostForPlagiarismAsync, model, OnDisposeCts.Token).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> TryCreateOrEditPostAsync(PreparePostModel model)
        {
            return await TaskHelper.TryRunTaskAsync(CreateOrEditPostAsync, model, OnDisposeCts.Token).ConfigureAwait(false);
        }

        private async Task<OperationResult<VoidResponse>> CreateOrEditPostAsync(PreparePostModel model, CancellationToken ct)
        {
            var operationResult = await _steepshotApiClient.PreparePostAsync(model, ct).ConfigureAwait(false);

            if (!operationResult.IsSuccess)
                return new OperationResult<VoidResponse>(operationResult.Exception);

            var preparedData = operationResult.Result;
            var meta = JsonConvert.SerializeObject(preparedData.JsonMetadata);
            var commentModel = new CommentModel(model, preparedData.Body, meta);
            if (!model.IsEditMode)
                commentModel.Beneficiaries = preparedData.Beneficiaries;

            var result = await _ditchClient.CreateOrEditAsync(commentModel, ct).ConfigureAwait(false);
            if (model.IsEditMode)
            {
                await _steepshotApiClient.TraceAsync($"post/{model.PostPermlink}/edit", model.Login, result.Exception, model.PostPermlink, ct).ConfigureAwait(false);
            }
            else
            {
                await _steepshotApiClient.TraceAsync("post", model.Login, result.Exception, model.PostPermlink, ct).ConfigureAwait(false);
            }

            var infoModel = new NamedInfoModel($"@{model.Author}/{model.Permlink}")
            {
                Login = model.Login,
                ShowLowRated = true,
                ShowNsfw = true
            };
            var postInfo = await _steepshotApiClient.GetPostInfoAsync(infoModel, ct).ConfigureAwait(false);

            return result;
        }

        public async Task<OperationResult<SpamResponse>> TryCheckForSpamAsync(string username)
        {
            return await TaskHelper.TryRunTaskAsync(_steepshotApiClient.CheckForSpamAsync, username, OnDisposeCts.Token).ConfigureAwait(false);
        }
    }
}
