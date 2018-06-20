using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cryptography.ECDSA;
using Ditch.Core.Errors;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Serializing;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.HttpClient
{
    internal abstract class BaseDitchClient
    {
        protected readonly JsonNetConverter JsonConverter;
        protected readonly object SyncConnection;


        public volatile bool EnableWrite;


        public abstract KnownChains Chain { get; }

        public abstract bool IsConnected { get; }


        protected BaseDitchClient(JsonNetConverter jsonConverter)
        {
            JsonConverter = jsonConverter;
            SyncConnection = new object();
        }


        public abstract Task<OperationResult<VoidResponse>> Vote(VoteModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> Follow(FollowModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> ValidatePrivateKey(ValidatePrivateKeyModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> CreateOrEdit(CommentModel model, CancellationToken ct);

        public abstract Task<OperationResult<object>> GetVerifyTransaction(AuthorizedPostingModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> Delete(DeleteModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> Transfer(TransferModel model, CancellationToken ct);

        public abstract Task<OperationResult<AccountInfoResponse>> GetAccountInfo(string userName, CancellationToken ct);

        public abstract bool TryReconnectChain(CancellationToken token);
        
        protected List<byte[]> ToKeyArr(string postingKey)
        {
            var key = ToKey(postingKey);
            if (key == null)
                return null;

            return new List<byte[]> { key };
        }

        protected byte[] ToKey(string postingKey)
        {
            try
            {
                var key = Ditch.Core.Base58.DecodePrivateWif(postingKey);
                if (key == null || key.Length != 32)
                    return null;
                return key;
            }
            catch (Exception)
            {
                //todo nothing
            }
            return null;
        }

        protected void OnError<T>(JsonRpcResponse response, OperationResult<T> operationResult)
        {
            if (response.IsError)
            {
                if (response.Error is SystemError systemError)
                {
                    operationResult.Error = new HttpError(systemError);

                }
                else if (response.Error is ResponseError responseError)
                {
                    operationResult.Error = new BlockchainError(responseError);
                }
                else
                {
                    operationResult.Error = new ServerError(response.Error);
                }
            }
        }

        protected string UpdateProfileJson(string jsonMetadata, UpdateUserProfileModel model)
        {
            var meta = string.IsNullOrEmpty(jsonMetadata) ? "{}" : jsonMetadata;
            var jMeta = JsonConverter.Deserialize<JObject>(meta);
            var jProfile = GetOrCreateJObject(jMeta, "profile");
            UpdateJValue(jProfile, "profile_image", model.ProfileImage);
            UpdateJValue(jProfile, "name", model.Name);
            UpdateJValue(jProfile, "location", model.Location);
            UpdateJValue(jProfile, "website", model.Website);
            UpdateJValue(jProfile, "about", model.About);
            return JsonConverter.Serialize(jMeta);
        }

        protected JObject GetOrCreateJObject(JObject jObject, string name)
        {
            var value = jObject.GetValue(name);
            if (value == null)
            {
                var obj = new JObject();
                jObject.Add(name, obj);
                return obj;
            }
            return (JObject)value;
        }

        protected void UpdateJValue(JObject jObject, string name, string newValue)
        {
            var value = jObject.GetValue(name);
            if (value == null)
            {
                if (string.IsNullOrEmpty(newValue))
                    return;

                jObject.Add(name, new JValue(newValue));
                return;
            }

            if (string.IsNullOrEmpty(newValue))
            {
                value.Remove();
                return;
            }
            value.Replace(new JValue(newValue));
        }
    }
}
