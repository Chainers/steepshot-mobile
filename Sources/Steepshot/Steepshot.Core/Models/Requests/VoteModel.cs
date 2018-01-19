using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VoteModel : AuthorizedModel
    {
        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string Identifier { get; private set; }

        [JsonProperty]
        [Required]
        public VoteType Type { get; }

        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public readonly string Permlink;

        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public readonly string Author;

        
        public VoteModel(UserInfo user, VoteType type, string identifier) : base(user)
        {
            Type = type;
            Identifier = identifier;

            UrlHelper.TryCastUrlToAuthorAndPermlink(Identifier, out Author, out Permlink);
        }
    }
}
