using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Serializing;
using Ditch.Core.JsonRpc;
using Ditch.Steem;
using Ditch.Steem.Models;
using Ditch.Steem.Operations;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Ditch.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.HttpClient
{
    internal class SteemClient : BaseDitchClient
    {
        private readonly OperationManager _operationManager;


        public override bool IsConnected => _operationManager.IsConnected;

        public override KnownChains Chain => KnownChains.Steem;

        public SteemClient(JsonNetConverter jsonConverter) : base(jsonConverter)
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
                    var cUrls = AppSettings.ConfigManager.SteemNodeConfigs
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

        private async Task<OperationResult<VoidResponse>> Broadcast(List<byte[]> keys, BaseOperation[] ops, CancellationToken ct)
        {
            var resp = await _operationManager.BroadcastOperationsSynchronousLikeSteemit(keys, ops, ct);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Error = new RequestError(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override async Task<OperationResult<VoidResponse>> Vote(VoteModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            short weigth = 0;
            if (model.Type == VoteType.Up)
                weigth = (short)(AppSettings.User.VotePower * 100);
            if (model.Type == VoteType.Flag)
                weigth = -10000;

            var op = new VoteOperation(model.Login, model.Author, model.Permlink, weigth);
            return await Broadcast(keys, new BaseOperation[] { op }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> Follow(FollowModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            var op = model.Type == Models.Enums.FollowType.Follow
                ? new FollowOperation(model.Login, model.Username, Ditch.Steem.Models.FollowType.Blog, model.Login)
                : new UnfollowOperation(model.Login, model.Username, model.Login);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
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
                    .Select(i => new Ditch.Steem.Operations.Beneficiary(i.Account, i.Weight))
                    .ToArray();
                ops = new BaseOperation[]
                {
                        op,
                        new BeneficiariesOperation(model.Login, model.Permlink,new Asset(1000000000, Config.SteemAssetNumSbd) ,beneficiaries)
                };
            }
            else
            {
                ops = new BaseOperation[] { op };
            }

            return await Broadcast(keys, ops, ct);
        }

        public override async Task<OperationResult<VoidResponse>> Delete(DeleteModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivatePostingKey));

            var op = new DeleteCommentOperation(model.Author, model.Permlink);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivateActimeKey));

            var args = new FindAccountsArgs
            {
                Accounts = new[] { model.Login }
            };
            var resp = await _operationManager.FindAccounts(args, CancellationToken.None);
            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
            {
                result.Error = new RequestError(resp);
                return result;
            }

            var profile = resp.Result.Accounts.Length == 1 ? resp.Result.Accounts[0] : null;
            if (profile == null)
            {
                result.Error = new ValidationError(LocalizationKeys.UnexpectedProfileData);
                return result;
            }

            var editedMeta = UpdateProfileJson(profile.JsonMetadata, model);

            var op = new AccountUpdateOperation(model.Login, profile.MemoKey, editedMeta);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> Transfer(TransferModel model, CancellationToken ct)
        {
            if (!TryReconnectChain(ct))
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.WrongPrivateActimeKey));

            var result = new OperationResult<VoidResponse>();

            Asset asset = new Asset();
            asset.NumberFormat = NumberFormatInfo.InvariantInfo;
            switch (model.CurrencyType)
            {
                case CurrencyType.Steem:
                    {
                        asset.FromOldFormat($"{model.Value} {Config.Steem}");
                        break;
                    }
                case CurrencyType.Sbd:
                    {
                        asset.FromOldFormat($"{model.Value} {Config.Sbd}");
                        break;
                    }
                default:
                    {
                        result.Error = new ValidationError(LocalizationKeys.UnsupportedCurrency, model.CurrencyType.ToString());
                        return result;
                    }
            }

            var t = asset.ToOldFormatString();
            var op = new TransferOperation(model.Login, model.Recipient, asset, model.Memo);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
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

            var op = new FollowOperation(model.Login, "steepshot", Ditch.Steem.Models.FollowType.Blog, model.Login);
            var properties = new DynamicGlobalPropertyObject
            {
                HeadBlockId = "0000000000000000000000000000000000000000",
                Time = DateTime.Now,
                HeadBlockNumber = 0
            };
            var tr = await _operationManager.CreateTransaction(properties, keys, op, ct);

            var conv = JsonConvert.SerializeObject(tr, _operationManager.CondenserJsonSerializerSettings);
            return new OperationResult<object> { Result = JsonConvert.DeserializeObject<JObject>(conv) };
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


            var args = new FindAccountsArgs
            {
                Accounts = new[] { model.Login }
            };
            var resp = await _operationManager.FindAccounts(args, CancellationToken.None);
            if (resp.IsError)
            {
                result.Error = new RequestError(resp);
                return result;
            }

            if (resp.Result.Accounts.Length != 1 || resp.Result.Accounts[0] == null)
            {
                return new OperationResult<VoidResponse>(new ValidationError(LocalizationKeys.UnexpectedProfileData));
            }

            Authority authority;

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    authority = resp.Result.Accounts[0].Active;
                    break;
                case KeyRoleType.Posting:
                    authority = resp.Result.Accounts[0].Posting;
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

            var args = new FindAccountsArgs
            {
                Accounts = new[] { userName }
            };
            var resp = await _operationManager.FindAccounts(args, CancellationToken.None);
            if (resp.IsError)
            {
                result.Error = new RequestError(resp);
                return result;
            }

            if (resp.Result.Accounts.Length != 1 || resp.Result.Accounts[0] == null)
            {
                return new OperationResult<AccountInfoResponse>(new ValidationError(LocalizationKeys.UnexpectedProfileData));
            }

            var acc = resp.Result.Accounts[0];
            result.Result = new AccountInfoResponse
            {
                PublicPostingKeys = acc.Posting.KeyAuths.Select(i => i.Key.Data).ToArray(),
                PublicActiveKeys = acc.Active.KeyAuths.Select(i => i.Key.Data).ToArray(),
                Metadata = JsonConverter.Deserialize<AccountMetadata>(acc.JsonMetadata)
            };

            result.Result.Balances = new List<BalanceModel>
            {
                new BalanceModel(acc.Balance.ToDoubleString(), 3, CurrencyType.Steem),
                new BalanceModel(acc.SbdBalance.ToDoubleString(), 3, CurrencyType.Sbd)
            };

            return result;
        }
        #endregion
    }
}
