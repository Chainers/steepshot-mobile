using System;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class CreateCommentRequest : AuthorizedRequest
    {
        public CreateCommentRequest(UserInfo user, string url, string body, string title) : base(user)
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
        }

        public string Url { get; }

        public string Body { get; }

        public string Title { get; }
    }
}