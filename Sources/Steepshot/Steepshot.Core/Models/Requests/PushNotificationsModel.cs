using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushNotificationsModel : AuthorizedModel
    {
        [JsonProperty("username")]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyLogin))]
        public string UserName { get; set; }
        [JsonProperty("trx")]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyVerifyTransaction))]
        public object VerifyTransaction { get; set; }
        [JsonProperty]
        [Required]
        public string AppId { get; } = "77fa644f-3280-4e87-9f14-1f0c7ddf8ca5";
        [JsonProperty]
        [Required]
        public string PlayerId { get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PushSubscription[] Subscriptions { get; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string WatchedUser { get; }
        [JsonIgnore]
        public bool Subscribe { get; }
        public PushNotificationsModel(UserInfo user, string playerId, bool subscribe) : base(user)
        {
            UserName = user.Login;
            PlayerId = playerId;
            Subscribe = subscribe;
        }
        public PushNotificationsModel(UserInfo user, string playerId, PushSubscription[] subscriptions, bool subscribe) : this(user, playerId, subscribe)
        {
            Subscriptions = subscriptions;
        }

        public PushNotificationsModel(UserInfo user, string playerId, string watchedUser, bool subscribe) : this(user, playerId, subscribe)
        {
            WatchedUser = watchedUser;
        }
    }
}
