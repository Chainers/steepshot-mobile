using System;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public enum VoteType
    {
        upvote,
        downvote
    }

    public class VoteRequest : LoginRequest
    {
        public VoteRequest(UserInfo user, bool isUp, string identifier) 
            : base(user)
        {
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            Type = isUp ? VoteType.upvote : VoteType.downvote;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [JsonIgnore]
        public VoteType Type { get; private set; }
    }
}