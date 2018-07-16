using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Serializing;
using Ditch.Core.JsonRpc;
using Ditch.Golos;
using Ditch.Golos.Models;
using Ditch.Golos.Operations;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Localization;
using Ditch.Core;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.HttpClient
{
    internal class GolosClient : BaseDitchClient
    {
        private readonly OperationManager _operationManager;


        public override bool IsConnected => _operationManager.IsConnected;

        public override KnownChains Chain => KnownChains.Golos;


        public GolosClient(JsonNetConverter jsonConverter) : base(jsonConverter)
        {
            var httpManager = new HttpManager();
            _operationManager = new OperationManager(httpManager);
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
                    var cUrls = AppSettings.ConfigManager.GolosNodeConfigs
                        .Where(n => n.IsEnabled)
                        .OrderBy(n => n.Order)
                        .Select(n => n.Url)
                        .ToArray();
                    foreach (var url in cUrls)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        if (_operationManager.ConnectTo(url, token))
                        {
                            EnableWrite = true;
                            break;
                        }
                    }
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

        public override async Task<OperationResult<VoidResponse>> Vote(VoteModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            short weigth = 0;
            if (model.Type == VoteType.Up)
                weigth = 10000;
            if (model.Type == VoteType.Flag)
                weigth = -10000;

            var op = new VoteOperation(model.Login, model.Author, model.Permlink, weigth);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Error = new RequestError(resp);
            else
                result.Result = new VoidResponse();

            return result;
        }

        public override async Task<OperationResult<VoidResponse>> Follow(FollowModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            var op = model.Type == Models.Enums.FollowType.Follow
                ? new FollowOperation(model.Login, model.Username, Ditch.Golos.Models.FollowType.Blog, model.Login)
                : new UnfollowOperation(model.Login, model.Username, model.Login);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);

            var result = new OperationResult<VoidResponse>();

            if (resp.IsError)
                result.Error = new RequestError(resp);
            else
                result.Result = new VoidResponse();

            return result;
        }

        public override async Task<OperationResult<VoidResponse>> CreateOrEdit(CommentModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            var op = new CommentOperation(model.ParentAuthor, model.ParentPermlink, model.Author, model.Permlink, model.Title, model.Body, model.JsonMetadata);

            BaseOperation[] ops;
            if (model.Beneficiaries != null && model.Beneficiaries.Any())
            {
                var beneficiaries = model.Beneficiaries
                    .Select(i => new Ditch.Golos.Operations.Beneficiary(i.Account, i.Weight))
                    .ToArray();
                ops = new BaseOperation[]
                {
                        op,
                        new BeneficiariesOperation(model.Login, model.Permlink, "GBG", beneficiaries)
                };
            }
            else
            {
                ops = new BaseOperation[] { op };
            }

            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, ops);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Error = new RequestError(resp);
            else
                result.Result = new VoidResponse();

            return result;
        }

        public override async Task<OperationResult<VoidResponse>> Delete(DeleteModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            var op = new DeleteCommentOperation(model.Author, model.Permlink);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Error = new RequestError(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override async Task<OperationResult<VoidResponse>> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivateActimeKey));

            var resp = await _operationManager.LookupAccountNames(new[] { model.Login }, CancellationToken.None);
            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
            {
                result.Error = new RequestError(resp);
                return result;
            }

            var profile = resp.Result.Length == 1 ? resp.Result[0] : null;
            if (profile == null)
            {
                result.Error = new ValidationError(LocalizationKeys.UnexpectedProfileData);
                return result;
            }

            var editedMeta = UpdateProfileJson(profile.JsonMetadata, model);

            var op = new AccountUpdateOperation(model.Login, profile.MemoKey, editedMeta);
            var resp2 = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);
            if (resp2.IsError)
                result.Error = new RequestError(resp2);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override async Task<OperationResult<VoidResponse>> Transfer(TransferModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivateActimeKey));

            var result = new OperationResult<VoidResponse>();

            Asset asset;
            switch (model.CurrencyType)
            {
                case CurrencyType.Golos:
                    {
                        asset = new Asset($"{model.Value} GOLOS");
                        break;
                    }
                case CurrencyType.Gbg:
                    {
                        asset = new Asset($"{model.Value} GBG");
                        break;
                    }
                default:
                    {
                        result.Error = new ValidationError(LocalizationKeys.UnsupportedCurrency, model.CurrencyType.ToString());
                        return result;
                    }
            }

            var op = new TransferOperation(model.Login, model.Recipient, asset, model.Memo);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);
            if (resp.IsError)
                result.Error = new RequestError(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        #endregion Post requests

        #region Get

        public override async Task<OperationResult<object>> GetVerifyTransaction(AuthorizedPostingModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<object>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<object>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            var op = new FollowOperation(model.Login, "steepshot", Ditch.Golos.Models.FollowType.Blog, model.Login);
            var properties = new DynamicGlobalPropertyObject
            {
                HeadBlockId = "0000000000000000000000000000000000000000",
                Time = DateTime.Now,
                HeadBlockNumber = 0
            };
            var tr = await _operationManager.CreateTransaction(properties, keys, op, ct);
            return new OperationResult<object>() { Result = tr };
        }

        public override async Task<OperationResult<VoidResponse>> ValidatePrivateKey(ValidatePrivateKeyModel model, CancellationToken ct)
        {
            var keys = ToKey(model.PrivateKey);
            if (keys == null)
            {
                switch (model.KeyRoleType)
                {
                    case KeyRoleType.Active:
                        return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivateActimeKey));
                    case KeyRoleType.Posting:
                        return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));
                }
            }

            if (!TryReconnectChain(ct))
            {
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new OperationResult<VoidResponse>();

            var resp = await _operationManager.LookupAccountNames(new[] { model.Login }, CancellationToken.None);
            if (resp.IsError)
            {
                result.Error = new RequestError(resp);
                return result;
            }

            if (resp.Result.Length != 1 || resp.Result[0] == null)
            {
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.UnexpectedProfileData));
            }

            Authority authority;

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    authority = resp.Result[0].Active;
                    break;
                case KeyRoleType.Posting:
                    authority = resp.Result[0].Posting;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var isSame = KeyHelper.ValidatePrivateKey(keys, authority.KeyAuths.Select(i => i.Key.Data).ToArray());

            if (isSame)
                return new OperationResult<VoidResponse>(new VoidResponse());

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivateActimeKey));
                default:
                    return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));
            }
        }

        public override async Task<OperationResult<AccountInfoResponse>> GetAccountInfo(string userName, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
            {
                return new OperationResult<AccountInfoResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new OperationResult<AccountInfoResponse>();

            var resp = await _operationManager.LookupAccountNames(new[] { userName }, CancellationToken.None);
            if (resp.IsError)
            {
                result.Error = new RequestError(resp);
                return result;
            }

            if (resp.Result.Length != 1 || resp.Result[0] == null)
            {
                return new OperationResult<AccountInfoResponse>(new ValidationError(LocalizationKeys.UnexpectedProfileData));
            }

            var acc = resp.Result[0];
            result.Result = new AccountInfoResponse
            {
                PublicPostingKeys = acc.Posting.KeyAuths.Select(i => i.Key.Data).ToArray(),
                PublicActiveKeys = acc.Active.KeyAuths.Select(i => i.Key.Data).ToArray(),
                Metadata = JsonConverter.Deserialize<AccountMetadata>(acc.JsonMetadata)
            };

            result.Result.Balances = new Dictionary<CurrencyType, BalanceModel>
                {
                    {CurrencyType.Steem, new BalanceModel(acc.Balance.ToDoubleString(), 3 )},
                    {CurrencyType.Sbd, new BalanceModel(acc.SbdBalance.ToDoubleString(), 3)}
                };

            return result;
        }
        #endregion
    }
}
