using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Services;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EditCommentModel : CommentModel
    {
        public string Url { get; set; }

        public EditCommentModel(UserInfo user, Post parentPost, Post post, string body, IAppInfo appInfo)
            : base(user.Login, user.PostingKey, parentPost.Author, parentPost.Permlink, post.Author, post.Permlink, string.Empty, body, $"{{\"app\": \"steepshot/v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t\"}}")
        {
            Url = post.Url;
        }
    }
}
