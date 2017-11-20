using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.Helpers;
using Ditch.Golos;
using Ditch.Golos.Helpers;
using Ditch.Golos.Operations.Get;
using Ditch.Golos.Operations.Post;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using DitchFollowType = Ditch.Golos.Operations.Enums.FollowType;
using DitchBeneficiary = Ditch.Golos.Operations.Post.Beneficiary;

namespace Steepshot.Core.HttpClient
{
    internal class GolosClient : BaseDitchClient
    {
        private readonly OperationManager _operationManager;

        public GolosClient(JsonNetConverter jsonConverter) : base(jsonConverter)
        {
            _operationManager = new OperationManager();
        }


        public override bool TryReconnectChain(CancellationToken token)
        {
            try
            {
                if (!EnableWrite)
                {
                    var cUrls = new List<string> { "wss://ws.golos.io" };
                    var conectedTo = _operationManager.TryConnectTo(cUrls, token);
                    if (!string.IsNullOrEmpty(conectedTo))
                        EnableWrite = true;
                }
            }
            catch (Exception)
            {
                //todo nothing
            }
            return EnableWrite;
        }

        #region Post requests

        public override async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationToken ct)
        {
            if (!EnableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<VoteResponse>(Localization.Errors.WrongPrivateKey);

            return await Task.Run(() =>
            {
                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(request.Identifier, out author, out permlink))
                    return new OperationResult<VoteResponse>(Localization.Errors.IncorrectIdentifier);

                short weigth = 0;
                if (request.Type == VoteType.Up)
                    weigth = 10000;
                if (request.Type == VoteType.Flag)
                    weigth = -10000;

                var op = new VoteOperation(request.Login, author, permlink, weigth);
                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<VoteResponse>();
                if (!resp.IsError)
                {
                    var dt = DateTime.Now;
                    var content = _operationManager.GetContent(author, permlink, ct);
                    if (!content.IsError)
                    {
                        //Convert Money type to double
                        result.Result = new VoteResponse(true)
                        {
                            NewTotalPayoutReward = content.Result.TotalPayoutValue + content.Result.CuratorPayoutValue + content.Result.PendingPayoutValue,
                            NetVotes = content.Result.NetVotes,
                            VoteTime = dt
                        };
                    }
                }
                else
                {
                    OnError(resp, result);
                }
                return result;
            }, ct);
        }

        public override async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationToken ct)
        {
            if (!EnableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<FollowResponse>(Localization.Errors.WrongPrivateKey);

            return await Task.Run(() =>
            {
                var op = request.Type == FollowType.Follow
                    ? new FollowOperation(request.Login, request.Username, DitchFollowType.Blog, request.Login)
                    : new UnfollowOperation(request.Login, request.Username, request.Login);
                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<FollowResponse>();

                if (!resp.IsError)
                    result.Result = new FollowResponse(true);
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        public override async Task<OperationResult<LoginResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationToken ct)
        {
            if (!EnableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<LoginResponse>(Localization.Errors.WrongPrivateKey);

            return await Task.Run(() =>
            {
                var op = new FollowOperation(request.Login, "steepshot", DitchFollowType.Blog, request.Login);
                var resp = _operationManager.VerifyAuthority(keys, ct, op);

                var result = new OperationResult<LoginResponse>();

                if (!resp.IsError)
                    result.Result = new LoginResponse(true);
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        public override async Task<OperationResult<CommentResponse>> CreateComment(CommentRequest request, CancellationToken ct)
        {
            if (!EnableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<CommentResponse>(Localization.Errors.WrongPrivateKey);

            return await Task.Run(() =>
            {
                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(request.Url, out author, out permlink))
                    return new OperationResult<CommentResponse>(Localization.Errors.IncorrectIdentifier);

                var op = new ReplyOperation(author, permlink, request.Login, request.Body,
                    $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");

                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<CommentResponse>();
                if (!resp.IsError)
                {
                    result.Result = new CommentResponse(true);
                    result.Result.Permlink = op.Permlink;
                }
                else
                    OnError(resp, result);
                return result;
            }, ct);
        }

        public override async Task<OperationResult<CommentResponse>> EditComment(CommentRequest request, CancellationToken ct)
        {
            if (!EnableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<CommentResponse>(Localization.Errors.WrongPrivateKey);

            return await Task.Run(() =>
            {
                string author;
                string commentPermlink;
                string parentAuthor;
                string parentPermlink;
                if (!TryCastUrlToAuthorPermlinkAndParentPermlink(request.Url, out author, out commentPermlink, out parentAuthor, out parentPermlink) || !string.Equals(author, request.Login))
                    return new OperationResult<CommentResponse>(Localization.Errors.IncorrectIdentifier);

                var op = new CommentOperation(parentAuthor, parentPermlink, author, commentPermlink, string.Empty,
                    request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");
                // var op = new ReplyOperation(author, permlink, request.Login, request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");

                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<CommentResponse>();
                if (!resp.IsError)
                {
                    result.Result = new CommentResponse(true);
                    result.Result.Permlink = op.Permlink;
                }
                else
                {
                    OnError(resp, result);
                }
                return result;
            }, ct);
        }

        public override async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, UploadResponse uploadResponse, CancellationToken ct)
        {
            if (!EnableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<ImageUploadResponse>(Localization.Errors.WrongPrivateKey);

            return await Task.Run(() =>
            {
                Transliteration.PrepareTags(request.Tags);

                var meta = uploadResponse.Meta.ToString();
                if (!string.IsNullOrWhiteSpace(meta))
                    meta = meta.Replace(Environment.NewLine, string.Empty);

                var category = request.Tags.Length > 0 ? request.Tags[0] : "steepshot";
                var post = new PostOperation(category, request.Login, request.Title, uploadResponse.Payload.Body, meta);
                BaseOperation[] ops;
                if (uploadResponse.Beneficiaries != null && uploadResponse.Beneficiaries.Any() && VersionHelper.GetHardfork(_operationManager.Version) > 16)
                {
                    var beneficiaries = uploadResponse.Beneficiaries
                        .Select(i => new DitchBeneficiary(i.Account, i.Weight))
                        .ToArray();
                    ops = new BaseOperation[]
                    {
                        post,
                        new BeneficiariesOperation(request.Login, post.Permlink, _operationManager.SbdSymbol, beneficiaries)
                    };
                }
                else
                {
                    ops = new BaseOperation[] { post };
                }

                var resp = _operationManager.BroadcastOperations(keys, ct, ops);

                var result = new OperationResult<ImageUploadResponse>();
                if (!resp.IsError)
                {
                    uploadResponse.Payload.Permlink = post.Permlink;
                    result.Result = uploadResponse.Payload;
                }
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        #endregion Post requests

        #region Get

        public override string GetVerifyTransaction(UploadImageRequest request, CancellationToken ct)
        {
            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return string.Empty;


            var op = new FollowOperation(request.Login, "steepshot", DitchFollowType.Blog, request.Login);
            var properties = new DynamicGlobalPropertyObject { HeadBlockId = Hex.ToString(_operationManager.ChainId), Time = DateTime.Now, HeadBlockNumber = 0 };
            var tr = _operationManager.CreateTransaction(properties, keys, ct, op);
            return JsonConverter.Serialize(tr);
        }

        #endregion
    }
}
