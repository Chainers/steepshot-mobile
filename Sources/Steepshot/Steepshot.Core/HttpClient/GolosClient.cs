using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.Helpers;
using Ditch.Golos;
using Ditch.Golos.Helpers;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using DitchFollowType = Ditch.Golos.Enums.FollowType;
using DitchBeneficiary = Ditch.Golos.Operations.Beneficiary;
using Ditch.Core;
using Ditch.Golos.Operations;
using Ditch.Golos.Objects;

namespace Steepshot.Core.HttpClient
{
    internal class GolosClient : BaseDitchClient
    {
        private readonly OperationManager _operationManager;

        public override bool IsConnected => _operationManager.IsConnected;

        public GolosClient(JsonNetConverter jsonConverter) : base(jsonConverter)
        {
            var jss = GetJsonSerializerSettings();
            //var cm = new HttpManager(jss);
            var cm = new WebSocketManager(jss);
            _operationManager = new OperationManager(cm, jss);
        }

        public override bool TryReconnectChain(CancellationToken token)
        {
            if (EnableWrite)
                return EnableWrite;

            var lockWasTaken = false;
            try
            {
                Monitor.Enter(SyncConnection, ref lockWasTaken);
                if (!EnableWrite)
                {
                    //var cUrls = new List<string> { "https://public-ws.golos.io" };
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
            finally
            {
                if (lockWasTaken)
                    Monitor.Exit(SyncConnection);
            }
            return EnableWrite;
        }

        #region Post requests

        public override async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoteResponse>(Localization.Errors.EnableConnectToBlockchain);

                var keys = ToKeyArr(request.PostingKey);
                if (keys == null)
                    return new OperationResult<VoteResponse>(Localization.Errors.WrongPrivateKey);

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
                        //Convert Asset type to double
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

        public override async Task<OperationResult<VoidResponse>> Follow(FollowRequest request, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(Localization.Errors.EnableConnectToBlockchain);

                var keys = ToKeyArr(request.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(Localization.Errors.WrongPrivateKey);

                var op = request.Type == FollowType.Follow
                    ? new FollowOperation(request.Login, request.Username, DitchFollowType.Blog, request.Login)
                    : new UnfollowOperation(request.Login, request.Username, request.Login);
                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<VoidResponse>();

                if (!resp.IsError)
                    result.Result = new VoidResponse(true);
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(Localization.Errors.EnableConnectToBlockchain);

                var keys = ToKeyArr(request.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(Localization.Errors.WrongPrivateKey);

                var op = new FollowOperation(request.Login, "steepshot", DitchFollowType.Blog, request.Login);
                var resp = _operationManager.VerifyAuthority(keys, ct, op);

                var result = new OperationResult<VoidResponse>();

                if (!resp.IsError)
                    result.Result = new VoidResponse(true);
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        public override async Task<OperationResult<CommentResponse>> CreateComment(CommentRequest request, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<CommentResponse>(Localization.Errors.EnableConnectToBlockchain);

                var keys = ToKeyArr(request.PostingKey);
                if (keys == null)
                    return new OperationResult<CommentResponse>(Localization.Errors.WrongPrivateKey);

                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(request.Url, out author, out permlink))
                    return new OperationResult<CommentResponse>(Localization.Errors.IncorrectIdentifier);

                var replyOperation = new ReplyOperation(author, permlink, request.Login, request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");

                BaseOperation[] ops;
                if (request.Beneficiaries != null && request.Beneficiaries.Any() && VersionHelper.GetHardfork(_operationManager.Version) > 16)
                {
                    var beneficiaries = request.Beneficiaries
                        .Select(i => new DitchBeneficiary(i.Account, i.Weight))
                        .ToArray();
                    ops = new BaseOperation[]
                    {
                        replyOperation,
                        new BeneficiariesOperation(request.Login, replyOperation.Permlink, _operationManager.SbdSymbol, beneficiaries)
                    };
                }
                else
                {
                    ops = new BaseOperation[] { replyOperation };
                }

                var resp = _operationManager.BroadcastOperations(keys, ct, ops);

                var result = new OperationResult<CommentResponse>();
                if (!resp.IsError)
                {
                    result.Result = new CommentResponse(true);
                    result.Result.Permlink = replyOperation.Permlink;
                }
                else
                    OnError(resp, result);
                return result;
            }, ct);
        }

        public override async Task<OperationResult<CommentResponse>> EditComment(CommentRequest request, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<CommentResponse>(Localization.Errors.EnableConnectToBlockchain);

                var keys = ToKeyArr(request.PostingKey);
                if (keys == null)
                    return new OperationResult<CommentResponse>(Localization.Errors.WrongPrivateKey);

                string author;
                string commentPermlink;
                string parentAuthor;
                string parentPermlink;
                if (!TryCastUrlToAuthorPermlinkAndParentPermlink(request.Url, out author, out commentPermlink, out parentAuthor, out parentPermlink) || !string.Equals(author, request.Login))
                    return new OperationResult<CommentResponse>(Localization.Errors.IncorrectIdentifier);

                var op = new CommentOperation(parentAuthor, parentPermlink, author, commentPermlink, string.Empty, request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");
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
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<ImageUploadResponse>(Localization.Errors.EnableConnectToBlockchain);

                var keys = ToKeyArr(request.PostingKey);
                if (keys == null)
                    return new OperationResult<ImageUploadResponse>(Localization.Errors.WrongPrivateKey);

                OperationHelper.PrepareTags(request.Tags);

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

        public override OperationResult<string> GetVerifyTransaction(UploadImageRequest request, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<string>(Localization.Errors.EnableConnectToBlockchain);

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<string>(Localization.Errors.WrongPrivateKey);


            var op = new FollowOperation(request.Login, "steepshot", DitchFollowType.Blog, request.Login);
            var properties = new DynamicGlobalPropertyObject { HeadBlockId = Hex.ToString(_operationManager.ChainId), Time = DateTime.Now, HeadBlockNumber = 0 };
            var tr = _operationManager.CreateTransaction(properties, keys, ct, op);
            return new OperationResult<string>() { Result = JsonConverter.Serialize(tr) };
        }

        #endregion
    }
}
