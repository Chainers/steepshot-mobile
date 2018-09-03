using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Extensions;

namespace Steepshot.Core.Models.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SubscriptionsModel
    {
        [JsonProperty("subscriptions")]
        public List<string> Subscriptions { get; set; }

        [JsonProperty("watched_users")]
        public List<string> WatchedUsers { get; set; }

        [JsonIgnore]
        public PushSettings EnumSubscriptions
        {
            get
            {
                var subscription = PushSettings.None;

                foreach (var item in Subscriptions)
                {
                    if (item == PushSettings.Upvote.GetEnumDescription())
                        subscription |= PushSettings.Upvote;
                    else if (item == PushSettings.UpvoteComment.GetEnumDescription())
                        subscription |= PushSettings.UpvoteComment;
                    else if (item == PushSettings.Follow.GetEnumDescription())
                        subscription |= PushSettings.Follow;
                    else if (item == PushSettings.Comment.GetEnumDescription())
                        subscription |= PushSettings.Comment;
                    else if (item == PushSettings.User.GetEnumDescription())
                        subscription |= PushSettings.User;
                    else if (item == PushSettings.Transfer.GetEnumDescription())
                        subscription |= PushSettings.Transfer;
                }

                return subscription;
            }
        }
    }
}
