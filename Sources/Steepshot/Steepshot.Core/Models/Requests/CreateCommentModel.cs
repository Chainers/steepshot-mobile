using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.Services;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CreateCommentModel : AuthorizedModel
    {
        [JsonProperty]
        public bool IsNeedRewards { get; }

        [Required(ErrorMessage = Localization.Errors.EmptyBodyField)]
        [JsonProperty]
        public string Body { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string ParentUrl { get; set; }

        [JsonProperty]
        public string JsonMetadata { get; }


        public CreateCommentModel(UserInfo user, string parentUrl, string body, IAppInfo appInfo) : base(user)
        {
            IsNeedRewards = user.IsNeedRewards;
            ParentUrl = parentUrl;
            Body = body;
            JsonMetadata = $"{{\"app\": \"steepshot/v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t\"}}";
        }
    }
}
