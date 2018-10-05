using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VoteModel : AuthorizedWifModel
    {
        public Post Post { get; }

        public string Permlink => Post.Permlink;

        public string Author => Post.Author;

        public bool IsComment => Post.IsComment;

        [JsonProperty]
        [Required]
        public VoteType Type { get; }


        public int VoteDelay { get; set; } = 3000;

        public short VotePower { get; set; }

        public VoteModel(UserInfo user, Post post, VoteType type) : base(user)
        {
            VotePower = user.VotePower;
            Post = post;
            Type = type;
        }
    }
}
