using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DeleteModel : CommentModel
    {
        [JsonProperty]
        public bool IsEnableToDelete { get; set; }

        public bool IsPost { get; }

        public DeleteModel(UserInfo user, Post post)
            : base(user.Login, user.PostingKey, string.Empty, post.Category, post.Author, post.Permlink, "*deleted*", "*deleted*", string.Empty)
        {
            IsPost = true;
            IsEnableToDelete = post.Children == 0 && post.NetLikes == 0;  // TODO:KOA: It`s not all case, research needed
        }

        public DeleteModel(UserInfo user, Post post, Post parentPost)
            : base(user.Login, user.PostingKey, parentPost.Author, parentPost.Permlink, post.Author, post.Permlink, string.Empty, "*deleted*", string.Empty)
        {
            IsPost = false;
            IsEnableToDelete = post.Children == 0 && post.NetLikes == 0; // TODO:KOA: It`s not all case, research needed
        }
    }
}
