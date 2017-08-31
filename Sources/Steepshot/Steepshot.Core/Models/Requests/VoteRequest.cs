using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public enum VoteType
    {
        [Display(Description = "upvote")]
        Up,

        [Display(Description = "downvote")]
        Down,

        [Display(Description = "flag")]
        Flag
    }

    public class VoteRequest : AuthorizedRequest
    {
        public VoteRequest(UserInfo user, VoteType type, string identifier) : base(user)
        {
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            Type = type;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [JsonIgnore]
        public VoteType Type { get; }
    }
}