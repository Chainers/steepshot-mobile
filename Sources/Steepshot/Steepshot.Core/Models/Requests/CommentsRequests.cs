using System;
using Steepshot.Core.Authority;
using Steepshot.Core.Services;

namespace Steepshot.Core.Models.Requests
{
    public class CreateCommentRequest : AuthorizedRequest
    {
        public CreateCommentRequest(UserInfo user, string url, string body, string title, IAppInfo appInfo) : base(user)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException(nameof(body), Localization.Errors.EmptyField);
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title), Localization.Errors.EmptyField);

            Url = url;
            Body = body;
            Title = title;
            AppVersion = $"v{appInfo.GetAppVersion()} b{appInfo.GetBuildVersion()} t";
        }

        public string Url { get; }

        public string Body { get; }

        public string Title { get; }

        public string AppVersion { get; }
    }
}