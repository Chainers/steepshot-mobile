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
            Type = type;
            Identifier = identifier;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [Required()]
        [JsonIgnore]
        public VoteType Type { get; }
    }
}
