using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public enum FlagType
    {
        [Display(Description = "flag")] Up,
        [Display(Description = "downvote")] Down
    }

    public class FlagRequest : AuthorizedRequest
    {
        public FlagRequest(UserInfo user, bool isUp, string identifier) : base(user)
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