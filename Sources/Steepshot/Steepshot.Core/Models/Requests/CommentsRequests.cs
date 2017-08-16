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
                throw new ArgumentNullException(nameof(body), "This field may not be blank!");
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title), "This field may not be blank!");

            Url = url;
            Body = body;
            Title = title;
        }

        public string Url { get; private set; }

        public string Body { get; private set; }

        public string Title { get; private set; }
    }
}