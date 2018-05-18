using System.Collections.Generic;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using Steepshot.Core.Utils;

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
        public string PlayerId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Subscriptions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string WatchedUser { get; set; }

        [JsonIgnore]
        public bool Subscribe { get; set; }

        public PushNotificationsModel(UserInfo user, string playerId, bool subscribe) : base(user)
        {
            UserName = AppSettings.User.Login;
            PlayerId = playerId;
            Subscribe = subscribe;
        }
    }
}
