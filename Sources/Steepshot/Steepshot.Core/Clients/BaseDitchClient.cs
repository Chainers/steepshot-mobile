using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    public abstract class BaseDitchClient
    {
        protected readonly ILogService LogService;
        protected readonly object SyncConnection;
        protected readonly ExtendedHttpClient ExtendedHttpClient;

        public volatile bool EnableWrite;

        public abstract KnownChains Chain { get; }

        public abstract bool IsConnected { get; }


        protected BaseDitchClient(ExtendedHttpClient extendedHttpClient, ILogService logService)
        {
            SyncConnection = new object();
            ExtendedHttpClient = extendedHttpClient;
            LogService = logService;
        }


        public abstract Task<OperationResult<VoidResponse>> VoteAsync(VoteModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> FollowAsync(FollowModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> ValidatePrivateKeyAsync(ValidatePrivateKeyModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> CreateOrEditAsync(CommentModel model, CancellationToken ct);

        public abstract Task<OperationResult<string>> GetVerifyTransactionAsync(AuthorizedWifModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> DeleteAsync(DeleteModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> UpdateUserProfileAsync(UpdateUserProfileModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> TransferAsync(TransferModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> PowerUpOrDownAsync(PowerUpDownModel model, CancellationToken ct);

        public abstract Task<OperationResult<VoidResponse>> ClaimRewardsAsync(ClaimRewardsModel model, CancellationToken ct);

        public abstract Task<OperationResult<AccountInfoResponse>> GetAccountInfoAsync(string userName, CancellationToken ct);

        public abstract Task<OperationResult<AccountHistoryResponse[]>> GetAccountHistoryAsync(AccountHistoryModel model, CancellationToken ct);

        public abstract Task<bool> TryReconnectChainAsync(CancellationToken token);


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
            catch (System.Exception ex)
            {
                LogService.WarningAsync(ex);
            }
            return null;
        }

        protected string UpdateProfileJson(string jsonMetadata, UpdateUserProfileModel model)
        {
            var meta = string.IsNullOrEmpty(jsonMetadata) ? "{}" : jsonMetadata;
            var jMeta = JsonConvert.DeserializeObject<JObject>(meta);
            var jProfile = GetOrCreateJObject(jMeta, "profile");
            UpdateJValue(jProfile, "profile_image", model.ProfileImage);
            UpdateJValue(jProfile, "name", model.Name);
            UpdateJValue(jProfile, "location", model.Location);
            UpdateJValue(jProfile, "website", model.Website);
            UpdateJValue(jProfile, "about", model.About);
            return JsonConvert.SerializeObject(jMeta);
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


        protected ValidationException Validate<T>(T request)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);
            Validator.TryValidateObject(request, context, results, true);
            if (results.Any())
            {
                var msg = results.Select(m => m.ErrorMessage).First();
                return new ValidationException(msg);
            }
            return null;
        }
    }
}
