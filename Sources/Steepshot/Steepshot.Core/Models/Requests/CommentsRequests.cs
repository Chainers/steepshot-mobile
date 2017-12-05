using Steepshot.Core.Authority;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Services;

namespace Steepshot.Core.Models.Requests
{
    public class CommentRequest : AuthorizedRequest
    {
        public CommentRequest(UserInfo user, string url, string body, IAppInfo appInfo) : base(user)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new UserException(Localization.Errors.EmptyUrlField);
            if (string.IsNullOrWhiteSpace(body))
                throw new UserException(Localization.Errors.EmptyCommentField);

            Url = url;
            Body = body;
            AppVersion = $"v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t";
        }

        public string Url { get; }

        public string Body { get; }

        public string AppVersion { get; }
    }
}
