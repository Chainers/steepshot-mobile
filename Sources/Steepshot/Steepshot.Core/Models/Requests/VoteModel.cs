using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VoteModel : AuthorizedPostingModel
    {
        public Post Post { get; }

        public string Permlink => Post.Permlink;

        public string Author => Post.Author;

        public bool IsComment => Post.IsComment;

        [JsonProperty]
        [Required]
        public VoteType Type { get; }


        public int VoteDelay { get; set; } = 3000;

        public VoteModel(UserInfo user, Post post, VoteType type) : base(user)
        {
            Post = post;
            Type = type;
        }
    }
}
