using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DeleteModel : CommentModel
    {
        [JsonProperty]
        public bool IsEnableToDelete { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public readonly string PostUrl;


        public DeleteModel(UserInfo user, Post post)
            : base(user.Login, user.PostingKey, string.Empty, post.Category, post.Author, post.Permlink, "*deleted*", "*deleted*", string.Empty)
        {
            IsEnableToDelete = post.Children == 0; // TODO:KOA: It`s not all case, research needed
            PostUrl = post.Url;
        }

        public DeleteModel(UserInfo user, Post post, Post parentPost)
            : base(user.Login, user.PostingKey, parentPost.Author, parentPost.Permlink, post.Author, post.Permlink, string.Empty, "*deleted*", string.Empty)
        {
            IsEnableToDelete = post.Children == 0; // TODO:KOA: It`s not all case, research needed
            PostUrl = post.Url;
        }
    }
}
