using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Sweetshot.Library.Models.Requests
{
    public enum VoteType
    {
        [Description("upvote")] Up,
        [Description("downvote")] Down
    }

    public class VoteRequest : LoginRequest
    {
        public VoteRequest(UserInfo user, bool isUp, string identifier) 
            : base(user)
        {
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            Type = isUp ? VoteType.Up : VoteType.Down;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [JsonIgnore]
        public VoteType Type { get; private set; }
    }
}