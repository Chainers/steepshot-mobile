using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public enum FlagType
    {
        flag,
        downvote
    }

    public class FlagRequest : LoginRequest
    {
        public FlagRequest(UserInfo user, bool isUp, string identifier)
            : base(user)
        {
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            Type = isUp ? FlagType.flag : FlagType.downvote;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [JsonIgnore]
        public FlagType Type { get; private set; }
    }
}