﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Steepshot.Core.Localization;
using System.ComponentModel.DataAnnotations;
using Steepshot.Core.Authorization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushNotificationsModel : AuthorizedPostingModel
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
        public List<string> Subscriptions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string WatchedUser { get; set; }

        [JsonIgnore]
        public bool Subscribe { get; }

        public PushNotificationsModel(UserInfo user, string playerId, bool subscribe) 
            : this(user, subscribe)
        {
            PlayerId = playerId;
        }

        public PushNotificationsModel(UserInfo user, bool subscribe) 
            : base(user)
        {
            UserName = user.Login;
            PlayerId = user.PushesPlayerId;
            Subscribe = subscribe;
        }
    }
}
