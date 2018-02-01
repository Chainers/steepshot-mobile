using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VoteModel : AuthorizedModel
    {
        public readonly string Permlink;

        public readonly string Author;

        [JsonProperty]
        [Required]
        public VoteType Type { get; }


        public VoteModel(UserInfo user, Post post, VoteType type) : base(user)
        {
            Type = type;
            Author = post.Author;
            Permlink = post.Permlink;
        }
    }
}
