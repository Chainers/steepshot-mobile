using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Jobs.Upload
{
    public class UploadMediaCommand : ICommand
    {
        private static SteepshotClient _steepshotClient;
        private static SteepshotApiClient _steepshotSteemApiClient;
        private static SteepshotApiClient _steepshotGolosApiClient;

        private readonly IConnectionService _connectionService;
        private readonly ILogService _logService;
        private readonly UserManager _userManager;
        private readonly DbManager _dbService;
        private readonly IFileProvider _fileProvider;

        public static readonly int Id = 1;

        public int CommandId => Id;

        public UploadMediaCommand(SteepshotClient steepshotClient, SteepshotApiClient steepshotSteemApiClient, SteepshotApiClient steepshotGolosApiClient,
            IConnectionService connectionService, ILogService logService, UserManager userManager, DbManager dbService, IFileProvider fileProvider)
        {
            _steepshotClient = steepshotClient;
            _steepshotSteemApiClient = steepshotSteemApiClient;
            _steepshotGolosApiClient = steepshotGolosApiClient;
            _connectionService = connectionService;
            _logService = logService;
            _userManager = userManager;
            _dbService = dbService;
            _fileProvider = fileProvider;
        }

        public async Task<JobState> Execute(int id, CancellationToken token)
        {
            var available = _connectionService.IsConnectionAvailable();
            if (!available)
                return JobState.Skipped;

            var media = _dbService.Select<UploadMediaContainer>(id);
            if (!IsValid(media, token))
                return JobState.Failed;

            var userInfo = _userManager.Select(media.Chain, media.Login);
            if (userInfo == null)
                return JobState.Failed;

            //if (string.IsNullOrEmpty(userInfo.Token))
            //{
            //    switch (userInfo.Chain)
            //    {
            //        case KnownChains.Steem:
            //            await _steemClient.AuthenticateAsync(userInfo, token);
            //            break;
            //        case KnownChains.Golos:
            //            await _golosClient.AuthenticateAsync(userInfo, token);
            //            break;
            //        default:
            //            await _steepshotClient.AuthenticateAsync(userInfo, token);
            //            break;
            //    }
            //}

            await UploadAsync(userInfo, media, token);
            await GetUploadMediaStatus(media, token);
            await GetMediaModel(media, token);

            if (media.Items.All(i => i.State == UploadState.Ready))
                return JobState.Ready;

            return JobState.Skipped;
        }

        private bool IsValid(UploadMediaContainer mediaContainer, CancellationToken token)
        {
            if (mediaContainer?.Items == null || mediaContainer.Items.Count == 0)
                return false;

            foreach (var item in mediaContainer.Items)
            {
                if (item.State == UploadState.None)
                    return false;

                if (item.State == UploadState.ReadyToUpload && !_fileProvider.Exist(item.FilePath))
                    return false;

                token.ThrowIfCancellationRequested();
            }

            return true;
        }

        private async Task UploadAsync(UserInfo userInfo, UploadMediaContainer mediaContainer, CancellationToken token)
        {
            for (var i = 0; i < mediaContainer.Items.Count; i++)
            {
                var media = mediaContainer.Items[i];
                if (media.State != UploadState.ReadyToUpload)
                    continue;

                var operationResult = await UploadMediaAsync(userInfo, media, token);

                if (operationResult.IsSuccess)
                {
                    media.State = UploadState.ReadyToVerify;
                    media.Uuid = operationResult.Result.Uuid;
                    SaveState(media);
                }
            }
        }

        private async Task<OperationResult<UUIDModel>> UploadMediaAsync(UserInfo userInfo, UploadMediaItem mediaItem, CancellationToken token)
        {
            System.IO.Stream stream = null;

            try
            {
                stream = _fileProvider.GetFileStream(mediaItem.FilePath);
                var mimeType = _fileProvider.GetMimeType(mediaItem.FilePath);
                var model = new UploadMediaModel(userInfo, stream)
                {
                    ContentType = mimeType
                };

                var serverResult = await _steepshotClient.UploadMediaAsync(model, token)
                    .ConfigureAwait(false);

                return serverResult;
            }
            catch (Exception ex)
            {
                await _logService.ErrorAsync(ex);
                return new OperationResult<UUIDModel>(ex);
            }
            finally
            {
                stream?.Flush();
                stream?.Dispose();
            }
        }

        private async Task GetUploadMediaStatus(UploadMediaContainer mediaContainer, CancellationToken token)
        {
            var steepshotApiClient = mediaContainer.Chain == KnownChains.Steem
                ? _steepshotSteemApiClient
                : _steepshotGolosApiClient;

            for (var i = 0; i < mediaContainer.Items.Count; i++)
            {
                var media = mediaContainer.Items[i];
                if (media.State != UploadState.ReadyToVerify)
                    continue;



                var operationResult = await steepshotApiClient.GetMediaStatusAsync(media.Uuid, token)
                    .ConfigureAwait(false);

                if (operationResult.IsSuccess)
                {
                    switch (operationResult.Result.Code)
                    {
                        case UploadMediaCode.Done:
                            {
                                media.State = UploadState.ReadyToResult;
                                SaveState(media);
                            }
                            break;
                        case UploadMediaCode.FailedToProcess:
                        case UploadMediaCode.FailedToUpload:
                        case UploadMediaCode.FailedToSave:
                            {
                                media.State = UploadState.ReadyToUpload;
                                SaveState(media);
                            }
                            break;
                    }
                }
            }
        }

        private async Task GetMediaModel(UploadMediaContainer mediaContainer, CancellationToken token)
        {
            var steepshotApiClient = mediaContainer.Chain == KnownChains.Steem
                ? _steepshotSteemApiClient
                : _steepshotGolosApiClient;

            for (var i = 0; i < mediaContainer.Items.Count; i++)
            {
                var media = mediaContainer.Items[i];
                if (media.State != UploadState.ReadyToResult)
                    continue;

                var mediaResult = await steepshotApiClient.GetMediaResultAsync(media.Uuid, token)
                    .ConfigureAwait(false);

                if (mediaResult.IsSuccess)
                {
                    media.ResultJson = mediaResult.RawResponse;
                    media.State = UploadState.Ready;
                    SaveState(media);
                }
            }
        }

        private void SaveState(UploadMediaItem mediaItem)
        {
            _dbService.Update(mediaItem);
        }

        public void CleanData(int id)
        {
            _dbService.Delete<UploadMediaContainer>(id);
        }

        public object GetResult(int id)
        {
            return _dbService.Select<UploadMediaContainer>(id);
        }
    }
}