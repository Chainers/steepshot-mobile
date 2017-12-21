using System.ComponentModel.DataAnnotations;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Services;

namespace Steepshot.Core.Models.Requests
{
    public class CommentRequest : AuthorizedRequest
    {
        public CommentRequest(UserInfo user, string url, string body, IAppInfo appInfo) : base(user)
        {
            Url = url;
            Body = body;
            AppVersion = $"v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t";
            IsNeedRewards = user.IsNeedRewards;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string Url { get; }

        [Required(ErrorMessage = Localization.Errors.EmptyCommentField)]
        public string Body { get; }

        public string AppVersion { get; }

        public bool IsNeedRewards { get; }

        public Beneficiary[] Beneficiaries { get; internal set; }
    }
}
