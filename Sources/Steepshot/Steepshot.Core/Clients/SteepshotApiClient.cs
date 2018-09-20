using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Clients
{
    public class SteepshotApiClient : BaseServerClient
    {
        private readonly Dictionary<KnownChains, Beneficiary[]> _beneficiariesCash;
        private readonly BaseDitchClient _ditchClient;
        public KnownChains Chain { get; }

        public SteepshotApiClient(ExtendedHttpClient extendedHttpClient, KnownChains chain)
        {
            HttpClient = extendedHttpClient;
            _beneficiariesCash = new Dictionary<KnownChains, Beneficiary[]>();
            Chain = chain;

            switch (chain)
            {
                case KnownChains.Steem:
                    BaseUrl = Constants.SteemUrl;
                    break;
                case KnownChains.Golos:
                    BaseUrl = Constants.GolosUrl;
                    break;
            }

            _ditchClient = chain == KnownChains.Steem
                ? (BaseDitchClient)new SteemClient(extendedHttpClient)
                : new GolosClient(extendedHttpClient);

            EnableRead = true;
        }

        public void SetDev(bool isDev)
        {
            switch (Chain)
            {
                case KnownChains.Steem when isDev:
                    BaseUrl = Constants.SteemUrlQa;
                    break;
                case KnownChains.Steem:
                    BaseUrl = Constants.SteemUrl;
                    break;
                case KnownChains.Golos when isDev:
                    BaseUrl = Constants.GolosUrlQa;
                    break;
                case KnownChains.Golos:
                    BaseUrl = Constants.GolosUrl;
                    break;
            }
        }

        public async Task<OperationResult<AccountInfoResponse>> GetAccountInfoAsync(string userName, CancellationToken ct)
        {
            return await _ditchClient.GetAccountInfoAsync(userName, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<AccountHistoryResponse[]>> GetAccountHistoryAsync(string userName, CancellationToken ct)
        {
            return await _ditchClient.GetAccountHistoryAsync(userName, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> ValidatePrivateKeyAsync(ValidatePrivateKeyModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var result = await _ditchClient.ValidatePrivateKeyAsync(model, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<OperationResult<Post>> VoteAsync(VoteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<Post>(results);

            var result = await _ditchClient.VoteAsync(model, ct).ConfigureAwait(false);

            var startDelay = DateTime.Now;

            await TraceAsync($"post/@{model.Author}/{model.Permlink}/{model.Type.GetDescription()}", model.Login, result.Exception, $"@{model.Author}/{model.Permlink}", ct).ConfigureAwait(false);
            if (!result.IsSuccess)
                return new OperationResult<Post>(result.Exception);

            OperationResult<Post> postInfo;
            if (model.IsComment) //TODO: << delete when comment update support will added on backend
            {
                postInfo = new OperationResult<Post> { Result = model.Post };
            }
            else
            {
                var infoModel = new NamedInfoModel($"@{model.Author}/{model.Permlink}")
                {
                    Login = model.Login,
                    ShowLowRated = true,
                    ShowNsfw = true
                };
                postInfo = await GetPostInfoAsync(infoModel, ct).ConfigureAwait(false);
            }

            var delay = (int)(model.VoteDelay - (DateTime.Now - startDelay).TotalMilliseconds);
            if (delay > 100)
                await Task.Delay(delay, ct).ConfigureAwait(false);

            return postInfo;
        }

        public async Task<OperationResult<VoidResponse>> FollowAsync(FollowModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var result = await _ditchClient.FollowAsync(model, ct).ConfigureAwait(false);
            await TraceAsync($"user/{model.Username}/{model.Type.ToString().ToLowerInvariant()}", model.Login, result.Exception, model.Username, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> CreateOrEditCommentAsync(CreateOrEditCommentModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            if (!model.IsEditMode)
            {
                var bKey = _ditchClient.Chain;
                if (_beneficiariesCash.ContainsKey(bKey))
                {
                    model.Beneficiaries = _beneficiariesCash[bKey];
                }
                else
                {
                    var beneficiaries = await GetBeneficiariesAsync(ct).ConfigureAwait(false);
                    if (beneficiaries.IsSuccess)
                        _beneficiariesCash[bKey] = model.Beneficiaries = beneficiaries.Result.Beneficiaries;
                }
            }

            var result = await _ditchClient.CreateOrEditAsync(model, ct).ConfigureAwait(false);
            //log parent post to perform update
            await TraceAsync($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, result.Exception, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct).ConfigureAwait(false);
            return result;
        }

        public async Task<OperationResult<PreparePostResponse>> CheckPostForPlagiarismAsync(PreparePostModel model, CancellationToken ct)
        {
            var result = await PreparePostAsync(model, ct).ConfigureAwait(false);

            if (!result.IsSuccess)
                return new OperationResult<PreparePostResponse>(result.Exception);

            return result;
        }

        public async Task<OperationResult<VoidResponse>> CreateOrEditPostAsync(PreparePostModel model, CancellationToken ct)
        {
            var operationResult = await PreparePostAsync(model, ct).ConfigureAwait(false);

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
                await TraceAsync($"post/{model.PostPermlink}/edit", model.Login, result.Exception, model.PostPermlink, ct).ConfigureAwait(false);
            }
            else
            {
                await TraceAsync("post", model.Login, result.Exception, model.PostPermlink, ct).ConfigureAwait(false);
            }

            var infoModel = new NamedInfoModel($"@{model.Author}/{model.Permlink}")
            {
                Login = model.Login,
                ShowLowRated = true,
                ShowNsfw = true
            };
            var postInfo = await GetPostInfoAsync(infoModel, ct).ConfigureAwait(false);

            return result;
        }

        public async Task<OperationResult<UUIDModel>> UploadMediaAsync(UploadMediaModel model, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<UUIDModel>(results);

            var endpoint = $"https://media.steepshot.org/api/v1/upload";
            return await HttpClient.UploadMediaAsync(endpoint, model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<UploadMediaStatusModel>> GetMediaStatusAsync(UUIDModel uuid, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/media/{uuid.Uuid}/status";
            return await HttpClient.GetAsync<UploadMediaStatusModel>(endpoint, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<MediaModel>> GetMediaResultAsync(UUIDModel model, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/media/{model.Uuid}/result";
            return await HttpClient.GetAsync<MediaModel>(endpoint, ct).ConfigureAwait(false);
        }



        public async Task<OperationResult<VoidResponse>> DeletePostOrCommentAsync(DeleteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            if (model.IsEnableToDelete)
            {
                var operationResult = await _ditchClient.DeleteAsync(model, ct).ConfigureAwait(false);
                if (operationResult.IsSuccess)
                {
                    //log parent post to perform update
                    if (model.IsPost)
                        await TraceAsync($"post/@{model.Author}/{model.Permlink}/delete", model.Login, operationResult.Exception, $"@{model.Author}/{model.Permlink}", ct).ConfigureAwait(false);
                    else
                        await TraceAsync($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, operationResult.Exception, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct).ConfigureAwait(false);

                    return operationResult;
                }
            }

            var result = await _ditchClient.CreateOrEditAsync(model, ct).ConfigureAwait(false);

            //log parent post to perform update
            if (model.IsPost)
                await TraceAsync($"post/@{model.Author}/{model.Permlink}/edit", model.Login, result.Exception, $"@{model.Author}/{model.Permlink}", ct).ConfigureAwait(false);
            else
                await TraceAsync($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, result.Exception, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct).ConfigureAwait(false);

            return result;
        }

        public async Task<OperationResult<VoidResponse>> UpdateUserProfileAsync(UpdateUserProfileModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            return await _ditchClient.UpdateUserProfileAsync(model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> UpdateUserPostsAsync(string username, CancellationToken ct)
        {
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{username}/update";
            var result = await HttpClient.GetAsync<VoidResponse>(endpoint, ct).ConfigureAwait(false);
            return result;
        }

        public async Task<OperationResult<object>> SubscribeForPushesAsync(PushNotificationsModel model, CancellationToken ct)
        {
            var trxResp = await _ditchClient.GetVerifyTransactionAsync(model, ct).ConfigureAwait(false);

            if (!trxResp.IsSuccess)
                return new OperationResult<object>(trxResp.Exception);

            model.VerifyTransaction = JsonConvert.DeserializeObject<JObject>(trxResp.Result);

            var results = Validate(model);
            if (results != null)
                return new OperationResult<object>(results);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/{(model.Subscribe ? "subscribe" : "unsubscribe")}";

            return await HttpClient.PutAsync<object, PushNotificationsModel>(endpoint, model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> TransferAsync(TransferModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            return await _ditchClient.TransferAsync(model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> PowerUpOrDownAsync(PowerUpDownModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            return await _ditchClient.PowerUpOrDownAsync(model, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> ClaimRewardsAsync(ClaimRewardsModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            return await _ditchClient.ClaimRewardsAsync(model, ct).ConfigureAwait(false);
        }
    }
}