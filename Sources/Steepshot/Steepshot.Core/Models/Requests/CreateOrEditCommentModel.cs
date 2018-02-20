using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Services;
using Steepshot.Core.Models.Common;
using Ditch.Core.Helpers;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CreateOrEditCommentModel : CommentModel
    {
        public bool IsEditMode { get; }

        public CreateOrEditCommentModel(UserInfo user, Post parentPost, string body, IAppInfo appInfo)
            : base(user, parentPost, OperationHelper.CreateReplyPermlink(user.Login, parentPost.Author, parentPost.Permlink), string.Empty, body, $"{{\"app\": \"steepshot/v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t\"}}")
        {
            IsEditMode = false;
        }

        public CreateOrEditCommentModel(UserInfo user, Post parentPost, Post post, string body, IAppInfo appInfo)
            : base(user, parentPost, post.Permlink, string.Empty, body, $"{{\"app\": \"steepshot/v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t\"}}")
        {
            IsEditMode = true;
        }
    }
}
