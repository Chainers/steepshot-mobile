using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Extensions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Database;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Jobs
{
    public class UploadMediaCommand : ICommand
    {
        private readonly IContainer _container;
        private readonly IConnectionService _connectionService;
        private readonly DbManager _dbManager;
        private readonly IFileProvider _fileProvider;
        private readonly ILogService _logService;
        private readonly SteepshotClient _steepshotClient;

        public static readonly string Id = "4832491F-7168-4F88-B781-1BF5BF662E72";

        public string CommandId => Id;

        public UploadMediaCommand(IContainer container, IConnectionService connectionService, DbManager dbManager, IFileProvider fileProvider, SteepshotClient steepshotClient, ILogService logService)
        {
            _container = container;
            _connectionService = connectionService;
            _dbManager = dbManager;
            _fileProvider = fileProvider;
            _steepshotClient = steepshotClient;
            _logService = logService;
        }

        public async Task<JobState> Execute(int id, CancellationToken token)
        {
            var available = _connectionService.IsConnectionAvailable();
            if (!available)
                return JobState.Skipped;

            var media = _dbManager.Select<UploadMediaContainer>(id);

            if (!IsValid(media, token))
                return JobState.Failed;

            var userManager = _container.GetUserManager();

            var userInfo = userManager.Select(media.Chain, media.Login);
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

            var steepshotApiClient = _container.GetSteepshotApiClient(media.Chain);

            await GetUploadMediaStatus(media, steepshotApiClient, token);

            await GetMediaModel(media, steepshotApiClient, token);

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

        private async Task GetUploadMediaStatus(UploadMediaContainer mediaContainer, SteepshotApiClient steepshotApiClient, CancellationToken token)
        {
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

        private async Task GetMediaModel(UploadMediaContainer mediaContainer, SteepshotApiClient steepshotApiClient, CancellationToken token)
        {
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
            _dbManager.Update(mediaItem);
        }

        public void CleanData(int id)
        {
            _dbManager.Delete<UploadMediaContainer>(id);
        }

        public object GetResult(int id)
        {
            return _dbManager.Select<UploadMediaContainer>(id);
        }
    }
}