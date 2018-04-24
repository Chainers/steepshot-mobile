using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using System.Linq;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Errors;

namespace Steepshot.Core.HttpClient
{
    public class SteepshotApiClient : BaseServerClient
    {
        private readonly Dictionary<string, Beneficiary[]> _beneficiariesCash;
        private readonly object _synk;

        private CancellationTokenSource _ctsMain;
        private BaseDitchClient _ditchClient;

        public SteepshotApiClient()
        {
            Gateway = new ApiGateway();
            JsonConverter = new JsonNetConverter();
            _beneficiariesCash = new Dictionary<string, Beneficiary[]>();
            _synk = new object();
        }

        public void InitConnector(KnownChains chain, bool isDev)
        {
            var sUrl = string.Empty;
            switch (chain)
            {
                case KnownChains.Steem when isDev:
                    sUrl = Constants.SteemUrlQa;
                    break;
                case KnownChains.Steem:
                    sUrl = Constants.SteemUrl;
                    break;
                case KnownChains.Golos when isDev:
                    sUrl = Constants.GolosUrlQa;
                    break;
                case KnownChains.Golos:
                    sUrl = Constants.GolosUrl;
                    break;
            }

            lock (_synk)
            {
                if (!string.IsNullOrEmpty(Gateway.BaseUrl))
                {
                    _ditchClient.EnableWrite = false;
                    _ctsMain.Cancel();
                }

                _ctsMain = new CancellationTokenSource();

                _ditchClient = chain == KnownChains.Steem
                    ? (BaseDitchClient)new SteemClient(JsonConverter)
                    : new GolosClient(JsonConverter);

                Gateway.BaseUrl = sUrl;
                EnableRead = true;
            }
        }

        public bool TryReconnectChain(CancellationToken token)
        {
            return _ditchClient.TryReconnectChain(token);
        }

        public async Task<OperationResult<VoidResponse>> LoginWithPostingKey(AuthorizedModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(results));

            var result = await _ditchClient.LoginWithPostingKey(model, ct);
            Trace("login-with-posting", model.Login, result.Error, string.Empty, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoteResponse>(new ValidationError(results));

            var result = await _ditchClient.Vote(model, ct);
            await Trace($"post/@{model.Author}/{model.Permlink}/{model.Type.GetDescription()}", model.Login, result.Error, $"@{model.Author}/{model.Permlink}", ct);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> Follow(FollowModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(results));

            var result = await _ditchClient.Follow(model, ct);
            await Trace($"user/{model.Username}/{model.Type.ToString().ToLowerInvariant()}", model.Login, result.Error, model.Username, ct);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> CreateOrEditComment(CreateOrEditCommentModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(results));

            if (!model.IsEditMode)
            {
                var bKey = $"{_ditchClient.GetType()}";
                if (_beneficiariesCash.ContainsKey(bKey))
                {
                    model.Beneficiaries = _beneficiariesCash[bKey];
                }
                else
                {
                    var beneficiaries = await GetBeneficiaries(ct);
                    if (beneficiaries.IsSuccess)
                        _beneficiariesCash[bKey] = model.Beneficiaries = beneficiaries.Result.Beneficiaries;
                }
            }

            var result = await _ditchClient.CreateOrEdit(model, ct);
            //log parent post to perform update
            await Trace($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, result.Error, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> CreateOrEditPost(PreparePostModel model, CancellationToken ct)
        {
            var operationResult = await PreparePost(model, ct);

            if (!operationResult.IsSuccess)
                return new OperationResult<VoidResponse>(operationResult.Error);

            var preparedData = operationResult.Result;
            var meta = JsonConverter.Serialize(preparedData.JsonMetadata);
            var commentModel = new CommentModel(model, preparedData.Body, meta);
            if (!model.IsEditMode)
                commentModel.Beneficiaries = preparedData.Beneficiaries;

            var result = await _ditchClient.CreateOrEdit(commentModel, ct);
            if (model.IsEditMode)
            {
                await Trace($"post/{model.PostPermlink}/edit", model.Login, result.Error, model.PostPermlink, ct);
            }
            else
            {
                await Trace("post", model.Login, result.Error, model.PostPermlink, ct);
            }
            return result;
        }

        public async Task<OperationResult<MediaModel>> UploadMedia(UploadMediaModel model, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<MediaModel>(new ValidationError(results));

            var trxResp = await _ditchClient.GetVerifyTransaction(model, ct);

            if (!trxResp.IsSuccess)
                return new OperationResult<MediaModel>(trxResp.Error);

            model.VerifyTransaction = JsonConverter.Serialize(trxResp.Result);

            var endpoint = $"{GatewayVersion.V1P1}/media/upload";
            return await Gateway.UploadMedia(endpoint, model, ct);
        }

        public async Task<OperationResult<VoidResponse>> DeletePostOrComment(DeleteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(results));

            if (model.IsEnableToDelete)
            {
                var operationResult = await _ditchClient.Delete(model, ct);
                if (operationResult.IsSuccess)
                {
                    if (model.IsPost)
                        await Trace($"post/@{model.Author}/{model.Permlink}/delete", model.Login, operationResult.Error, $"@{model.Author}/{model.Permlink}", ct);
                    return operationResult;
                }
            }

            var result = await _ditchClient.CreateOrEdit(model, ct);
            if (model.IsPost)
                await Trace($"post/@{model.Author}/{model.Permlink}/edit", model.Login, result.Error, $"@{model.Author}/{model.Permlink}", ct);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(results));

            return await _ditchClient.UpdateUserProfile(model, ct);
        }

        public async Task<OperationResult<object>> SubscribeForPushes(PushNotificationsModel model, CancellationToken ct)
        {
            var trxResp = await _ditchClient.GetVerifyTransaction(model, ct);

            if (!trxResp.IsSuccess)
                return new OperationResult<object>(trxResp.Error);

            model.VerifyTransaction = trxResp.Result;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<object>(new ValidationError(results));

            var endpoint = $"{GatewayVersion.V1P1}/{(model.Subscribe ? "subscribe" : "unsubscribe")}";

            return await Gateway.Post<object, PushNotificationsModel>(endpoint, model, ct);
        }
    }
}