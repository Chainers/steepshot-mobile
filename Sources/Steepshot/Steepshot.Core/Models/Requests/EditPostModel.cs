using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EditPostModel : CommentModel
    {
        public string Url { get; set; }

        public EditPostModel(UserInfo user, Post post, string title, string body, string jsonMetadata)
            : base(user.Login, user.PostingKey, string.Empty, post.Category, post.Author, post.Permlink, title, body, jsonMetadata)
        {
            Url = post.Url;
        }
    }
}
