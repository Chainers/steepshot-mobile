using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using System.Linq;
using Ditch.Core.Helpers;
using Steepshot.Core.Errors;
using Steepshot.Core.Utils;

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

        public void InitConnector(KnownChains chain, bool isDev, CancellationToken token)
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
                if (!string.IsNullOrEmpty(Gateway.Url))
                {
                    _ditchClient.EnableWrite = false;
                    _ctsMain.Cancel();
                }

                _ctsMain = new CancellationTokenSource();

                _ditchClient = chain == KnownChains.Steem
                    ? (BaseDitchClient)new SteemClient(JsonConverter)
                    : new GolosClient(JsonConverter);

                Gateway.Url = sUrl;
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
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.LoginWithPostingKey(model, ct);
            Trace("login-with-posting", model.Login, result.Error, string.Empty, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoteResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.Vote(model, ct);
            Trace($"post/{model.Identifier}/{model.Type.GetDescription()}", model.Login, result.Error, model.Identifier, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> Follow(FollowModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.Follow(model, ct);
            Trace($"user/{model.Username}/{model.Type.ToString().ToLowerInvariant()}", model.Login, result.Error, model.Username, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> CreateComment(CreateCommentModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            if (!UrlHelper.TryCastUrlToAuthorAndPermlink(model.ParentUrl, out var parentAuthor, out var parentPermlink))
                return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.IncorrectIdentifier));

            var permlink = OperationHelper.CreateReplyPermlink(model.Login, parentAuthor, parentPermlink);
            var commentModel = new CommentModel(model.Login, model.PostingKey, parentAuthor, parentPermlink, model.Login, permlink, string.Empty, model.Body, model.JsonMetadata);

            var bKey = $"{_ditchClient.GetType()}{model.IsNeedRewards}";
            if (_beneficiariesCash.ContainsKey(bKey))
            {
                commentModel.Beneficiaries = _beneficiariesCash[bKey];
            }
            else
            {
                var beneficiaries = await GetBeneficiaries(model.IsNeedRewards, ct);
                if (beneficiaries.IsSuccess)
                    _beneficiariesCash[bKey] = commentModel.Beneficiaries = beneficiaries.Result.Beneficiaries;
            }

            var result = await _ditchClient.Create(commentModel, ct);
            Trace($"post/{permlink}/comment", model.Login, result.Error, permlink, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> CreatePost(PreparePostModel model, CancellationToken ct)
        {
            var operationResult = await PreparePost(model, ct);

            if (!operationResult.IsSuccess)
                return new OperationResult<VoidResponse>(operationResult.Error);

            var preparedData = operationResult.Result;

            var category = model.Tags.Length > 0 ? model.Tags[0] : "steepshot";
            var commentModel = new CommentModel(model.Login, model.PostingKey, string.Empty, category, model.Login, model.PostPermlink, model.Title, preparedData.Body, preparedData.JsonMetadata)
            {
                Beneficiaries = preparedData.Beneficiaries
            };

            var result = await _ditchClient.Create(commentModel, ct);

            Trace("post", model.Login, result.Error, model.PostPermlink, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<Media>> UploadMedia(UploadMediaModel model, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<Media>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var trxResp = await _ditchClient.GetVerifyTransaction(model, ct);

            if (!trxResp.IsSuccess)
                return new OperationResult<Media>(trxResp.Error);

            model.VerifyTransaction = trxResp.Result;

            return await Gateway.UploadMedia(GatewayVersion.V1P1, "media/upload", model, ct);
        }

        public async Task<OperationResult<VoidResponse>> DeletePostOrComment(DeleteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            if (model.IsEnableToDelete)
            {
                var operationResult = await _ditchClient.Delete(model, ct);
                if (operationResult.IsSuccess)
                {
                    Trace("post", model.Login, operationResult.Error, model.PostUrl, ct);//.Wait(5000);\
                    return operationResult;
                }
            }

            var result = await _ditchClient.Edit(model, ct);
            Trace("post", model.Login, result.Error, model.PostUrl, ct);//.Wait(5000);\
            return result;
        }

        public async Task<OperationResult<VoidResponse>> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _ditchClient.UpdateUserProfile(model, ct);
        }

        public async Task<OperationResult<VoidResponse>> EditComment(EditCommentModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));


            var result = await _ditchClient.Edit(model, ct);
            Trace($"post/{model.Url}/comment", model.Login, result.Error, model.Url, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> EditPost(EditPostModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.Edit(model, ct);
            Trace($"post/{model.Url}/comment", model.Login, result.Error, model.Url, ct);//.Wait(5000);
            return result;
        }
    }
}