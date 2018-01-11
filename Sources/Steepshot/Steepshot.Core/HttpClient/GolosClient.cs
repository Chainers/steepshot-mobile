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
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;

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

        public override async Task<OperationResult<VoteResponse>> Vote(VoteModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoteResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<VoteResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(model.Identifier, out author, out permlink))
                    return new OperationResult<VoteResponse>(new ApplicationError(Localization.Errors.IncorrectIdentifier));

                short weigth = 0;
                if (model.Type == VoteType.Up)
                    weigth = 10000;
                if (model.Type == VoteType.Flag)
                    weigth = -10000;

                var op = new VoteOperation(model.Login, author, permlink, weigth);
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

        public override async Task<OperationResult<VoidResponse>> Follow(FollowModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                var op = model.Type == FollowType.Follow
                    ? new FollowOperation(model.Login, model.Username, DitchFollowType.Blog, model.Login)
                    : new UnfollowOperation(model.Login, model.Username, model.Login);
                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<VoidResponse>();

                if (!resp.IsError)
                    result.Result = new VoidResponse(true);
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> LoginWithPostingKey(AuthorizedModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                var op = new FollowOperation(model.Login, "steepshot", DitchFollowType.Blog, model.Login);
                var resp = _operationManager.VerifyAuthority(keys, ct, op);

                var result = new OperationResult<VoidResponse>();

                if (!resp.IsError)
                    result.Result = new VoidResponse(true);
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        public override async Task<OperationResult<CommentResponse>> CreateComment(CommentModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<CommentResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<CommentResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(model.Url, out author, out permlink))
                    return new OperationResult<CommentResponse>(new ApplicationError(Localization.Errors.IncorrectIdentifier));

                var replyOperation = new ReplyOperation(author, permlink, model.Login, model.Body, $"{{\"app\": \"steepshot/{model.AppVersion}\"}}");

                BaseOperation[] ops;
                if (model.Beneficiaries != null && model.Beneficiaries.Any() && VersionHelper.GetHardfork(_operationManager.Version) > 16)
                {
                    var beneficiaries = model.Beneficiaries
                        .Select(i => new DitchBeneficiary(i.Account, i.Weight))
                        .ToArray();
                    ops = new BaseOperation[]
                    {
                        replyOperation,
                        new BeneficiariesOperation(model.Login, replyOperation.Permlink, _operationManager.SbdSymbol, beneficiaries)
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

        public override async Task<OperationResult<CommentResponse>> EditComment(CommentModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<CommentResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<CommentResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                string author;
                string commentPermlink;
                string parentAuthor;
                string parentPermlink;
                if (!TryCastUrlToAuthorPermlinkAndParentPermlink(model.Url, out author, out commentPermlink, out parentAuthor, out parentPermlink) || !string.Equals(author, model.Login))
                    return new OperationResult<CommentResponse>(new ApplicationError(Localization.Errors.IncorrectIdentifier));

                var op = new CommentOperation(parentAuthor, parentPermlink, author, commentPermlink, string.Empty, model.Body, $"{{\"app\": \"steepshot/{model.AppVersion}\"}}");
                // var op = new ReplyOperation(author, permlink, model.Login, model.Body, $"{{\"app\": \"steepshot/{model.AppVersion}\"}}");

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

        public override async Task<OperationResult<ImageUploadResponse>> CreatePost(UploadImageModel model, UploadResponse uploadResponse, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<ImageUploadResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<ImageUploadResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                OperationHelper.PrepareTags(model.Tags);

                var meta = uploadResponse.Meta.ToString();
                if (!string.IsNullOrWhiteSpace(meta))
                    meta = meta.Replace(Environment.NewLine, string.Empty);

                var category = model.Tags.Length > 0 ? model.Tags[0] : "steepshot";
                var post = new PostOperation(category, model.Login, model.PostUrl, model.Title, uploadResponse.Payload.Body, meta);
                BaseOperation[] ops;
                if (uploadResponse.Beneficiaries != null && uploadResponse.Beneficiaries.Any() && VersionHelper.GetHardfork(_operationManager.Version) > 16)
                {
                    var beneficiaries = uploadResponse.Beneficiaries
                        .Select(i => new DitchBeneficiary(i.Account, i.Weight))
                        .ToArray();
                    ops = new BaseOperation[]
                    {
                        post,
                        new BeneficiariesOperation(model.Login, post.Permlink, _operationManager.SbdSymbol, beneficiaries)
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

        public override async Task<OperationResult<VoidResponse>> DeletePostOrComment(DeleteModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                if (!TryCastUrlToAuthorAndPermlink(model.Url, out var author, out var permlink) ||
                    !string.Equals(author, model.Login))
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.IncorrectIdentifier));

                var op = new DeleteCommentOperation(author, permlink);
                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<VoidResponse>();
                if (!resp.IsError)
                    result.Result = new VoidResponse(true);
                else
                    OnError(resp, result);
                return result;
            }, ct);
        }

        #endregion Post requests

        #region Get

        public override async Task<OperationResult<string>> GetVerifyTransaction(UploadImageModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<string>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<string>(new ApplicationError(Localization.Errors.WrongPrivateKey));

            return await Task.Run(() =>
            {
                var op = new FollowOperation(model.Login, "steepshot", DitchFollowType.Blog, model.Login);
                var properties = new DynamicGlobalPropertyObject
                {
                    HeadBlockId = Hex.ToString(_operationManager.ChainId),
                    Time = DateTime.Now,
                    HeadBlockNumber = 0
                };
                var tr = _operationManager.CreateTransaction(properties, keys, ct, op);
                return new OperationResult<string>() { Result = JsonConverter.Serialize(tr) };
            }, ct);
        }

        #endregion
    }
}
