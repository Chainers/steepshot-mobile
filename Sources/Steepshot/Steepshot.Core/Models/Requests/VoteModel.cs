using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VoteModel : AuthorizedModel
    {
        public VoteModel(UserInfo user, VoteType type, string identifier) : base(user)
        {
            Type = type;
            Identifier = identifier;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        [JsonProperty()]
        public string Identifier { get; private set; }

        [JsonProperty()]
        [Required()]
        public VoteType Type { get; }
    }
}
