using System;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    [Obsolete]
    public class SteepshotApiClient : BaseServerClient
    {
        public SteepshotApiClient(ExtendedHttpClient extendedHttpClient, ILogService logService, string baseUrl)
            : base(extendedHttpClient, logService, baseUrl)
        {
        }

        public async Task<OperationResult<PreparePostResponse>> CheckPostForPlagiarismAsync(PreparePostModel model, CancellationToken ct)
        {
            var result = await PreparePostAsync(model, ct).ConfigureAwait(false);

            if (!result.IsSuccess)
                return new OperationResult<PreparePostResponse>(result.Exception);

            return result;
        }

        public async Task<OperationResult<UploadMediaStatusModel>> GetMediaStatusAsync(UUIDModel model, CancellationToken ct)
        {
            return await GetMediaStatusAsync(model.Uuid, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<UploadMediaStatusModel>> GetMediaStatusAsync(string uuid, CancellationToken ct)
        {
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/media/{uuid}/status";
            return await HttpClient.GetAsync<UploadMediaStatusModel>(endpoint, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<MediaModel>> GetMediaResultAsync(UUIDModel model, CancellationToken ct)
        {
            return await GetMediaResultAsync(model.Uuid, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<MediaModel>> GetMediaResultAsync(string uuid, CancellationToken ct)
        {
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/media/{uuid}/result";
            return await HttpClient.GetAsync<MediaModel>(endpoint, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> UpdateUserPostsAsync(string username, CancellationToken ct)
        {
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{username}/update";
            var result = await HttpClient.GetAsync<VoidResponse>(endpoint, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<OperationResult<object>> SubscribeForPushesAsync(PushNotificationsModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<object>(results);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/{(model.Subscribe ? "subscribe" : "unsubscribe")}";

            return await HttpClient.PutAsync<object, PushNotificationsModel>(endpoint, model, ct).ConfigureAwait(false);
        }
    }
}