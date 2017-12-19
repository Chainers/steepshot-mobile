using Steepshot.Core.Authority;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Services;

namespace Steepshot.Core.Models.Requests
{
    public class CommentRequest : AuthorizedRequest
    {
        public CommentRequest(UserInfo user, string url, string body, IAppInfo appInfo) : base(user)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new UserException("url", Localization.Errors.EmptyUrlField);
            if (string.IsNullOrWhiteSpace(body))
                throw new UserException("body", Localization.Errors.EmptyCommentField);

            Url = url;
            Body = body;
            AppVersion = $"v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t";
            IsNeedRewards = user.IsNeedRewards;
        }

        public string Url { get; }

        public string Body { get; }

        public string AppVersion { get; }

        public bool IsNeedRewards { get; }

        public Beneficiary[] Beneficiaries { get; internal set; }
    }
}
