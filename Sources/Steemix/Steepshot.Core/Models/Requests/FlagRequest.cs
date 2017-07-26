using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Sweetshot.Library.Models.Requests
{
    public enum FlagType
    {
        [Description("flag")] Up,
        [Description("downvote")] Down
    }

    public class FlagRequest : LoginRequest
    {
        public FlagRequest(UserInfo user, bool isUp, string identifier)
            : base(user)
        {
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            Type = isUp ? FlagType.Up : FlagType.Down;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [JsonIgnore]
        public FlagType Type { get; private set; }
    }
}