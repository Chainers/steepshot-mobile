using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;
using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DeleteModel : AuthorizedModel
    {
        [JsonProperty]
        public bool IsEnableToDelete { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public readonly string ParentPermlink;

        [JsonProperty]
        public readonly string ParentAuthor;

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public readonly string Permlink;

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public readonly string Author;

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public readonly string PostUrl;


        public DeleteModel(UserInfo user, Post post) : base(user)
        {
            IsEnableToDelete = post.Children == 0; // TODO:KOA: It`s not all case, research needed
            ParentPermlink = post.Category;
            UrlHelper.TryCastUrlToAuthorAndPermlink(post.Url, out Author, out Permlink);
            PostUrl = post.Url;
        }

        public DeleteModel(UserInfo user, Post post, Post parentPost) : base(user)
        {
            IsEnableToDelete = post.Children == 0; // TODO:KOA: It`s not all case, research needed

            UrlHelper.TryCastUrlToAuthorAndPermlink(parentPost.Url, out ParentAuthor, out ParentPermlink);
            UrlHelper.TryCastUrlToAuthorAndPermlink(post.Url, out Author, out Permlink);
            PostUrl = post.Url;
        }
    }
}
