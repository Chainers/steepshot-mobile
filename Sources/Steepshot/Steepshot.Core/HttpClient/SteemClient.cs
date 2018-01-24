using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.Helpers;
using Ditch.Steem;
using Ditch.Steem.Operations;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using DitchFollowType = Ditch.Steem.Enums.FollowType;
using DitchBeneficiary = Ditch.Steem.Operations.Beneficiary;
using Ditch.Core;
using Ditch.Steem.Objects;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Core.HttpClient
{
    internal class SteemClient : BaseDitchClient
    {
        private readonly OperationManager _operationManager;

        public override bool IsConnected => _operationManager.IsConnected;

        public SteemClient(JsonNetConverter jsonConverter) : base(jsonConverter)
        {
            var jss = GetJsonSerializerSettings();
            var cm = new HttpManager(jss);
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
                    var cUrls = new List<string> { "https://api.steemit.com", "https://steemd2.steepshot.org" };
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

                short weigth = 0;
                if (model.Type == VoteType.Up)
                    weigth = 10000;
                if (model.Type == VoteType.Flag)
                    weigth = -10000;

                var op = new VoteOperation(model.Login, model.Author, model.Permlink, weigth);
                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<VoteResponse>();
                if (!resp.IsError)
                {
                    var dt = DateTime.Now;
                    var content = _operationManager.GetContent(model.Author, model.Permlink, ct);
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

        public override async Task<OperationResult<VoidResponse>> Edit(CommentModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                var op = new CommentOperation(model.ParentAuthor, model.ParentPermlink, model.Author, model.Permlink, model.Title, model.Body, model.JsonMetadata);
                var resp = _operationManager.BroadcastOperations(keys, ct, op);

                var result = new OperationResult<VoidResponse>();
                if (!resp.IsError)
                {
                    result.Result = new VoidResponse(true);
                }
                else
                {
                    OnError(resp, result);
                }
                return result;
            }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> Create(CommentModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                var op = new CommentOperation(model.ParentAuthor, model.ParentPermlink, model.Login, model.Permlink, model.Title, model.Body, model.JsonMetadata);

                BaseOperation[] ops;
                if (model.Beneficiaries != null && model.Beneficiaries.Any())
                {
                    var beneficiaries = model.Beneficiaries
                        .Select(i => new DitchBeneficiary(i.Account, i.Weight))
                        .ToArray();
                    ops = new BaseOperation[]
                    {
                        op,
                        new BeneficiariesOperation(model.Login, model.Permlink, _operationManager.SbdSymbol, beneficiaries)
                    };
                }
                else
                {
                    ops = new BaseOperation[] { op };
                }

                var resp = _operationManager.BroadcastOperations(keys, ct, ops);

                var result = new OperationResult<VoidResponse>();
                if (!resp.IsError)
                {
                    result.Result = new VoidResponse(true);
                }
                else
                    OnError(resp, result);

                return result;
            }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> Delete(DeleteModel model, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                if (!TryReconnectChain(ct))
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.EnableConnectToBlockchain));

                var keys = ToKeyArr(model.PostingKey);
                if (keys == null)
                    return new OperationResult<VoidResponse>(new ApplicationError(Localization.Errors.WrongPrivateKey));

                var op = new DeleteCommentOperation(model.Author, model.Permlink);
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
                var properties = new DynamicGlobalPropertyApiObj
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
