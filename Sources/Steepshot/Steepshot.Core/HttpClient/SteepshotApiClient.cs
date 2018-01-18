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

        public async Task<OperationResult<CommentResponse>> CreateComment(CommentModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<CommentResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var bKey = $"{_ditchClient.GetType()}{model.IsNeedRewards}";
            if (_beneficiariesCash.ContainsKey(bKey))
            {
                model.Beneficiaries = _beneficiariesCash[bKey];
            }
            else
            {
                var beneficiaries = await GetBeneficiaries(model.IsNeedRewards, ct);
                if (beneficiaries.IsSuccess)
                    _beneficiariesCash[bKey] = model.Beneficiaries = beneficiaries.Result.Beneficiaries;
            }

            var result = await _ditchClient.CreateComment(model, ct);
            Trace($"post/{model.Url}/comment", model.Login, result.Error, model.Url, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<CommentResponse>> EditComment(CommentModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<CommentResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.EditComment(model, ct);
            Trace($"post/{model.Url}/comment", model.Login, result.Error, model.Url, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<ImageUploadResponse>> CreatePost(UploadImageModel model, UploadResponse uploadResponse, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ImageUploadResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.CreatePost(model, uploadResponse, ct);
            Trace("post", model.Login, result.Error, uploadResponse.Payload.Permlink, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<UploadResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var trxResp = await _ditchClient.GetVerifyTransaction(model, ct);

            if (!trxResp.IsSuccess)
                return new OperationResult<UploadResponse>(trxResp.Error);

            model.VerifyTransaction = trxResp.Result;
            var response = await Upload(model, ct);

            if (response.IsSuccess)
                response.Result.PostUrl = model.PostUrl;
            return response;
        }

        public async Task<OperationResult<VoidResponse>> DeletePostOrComment(DeleteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var response = await _ditchClient.DeletePostOrComment(model, ct);
            // if (response.IsSuccess)
            return response;
        }
    }
}