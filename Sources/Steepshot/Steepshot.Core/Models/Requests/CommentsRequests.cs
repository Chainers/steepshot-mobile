using System;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class CreateCommentRequest : AuthorizedRequest
    {
        public CreateCommentRequest(UserInfo user, string url, string body, string title) : base(user)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
            
            Url = url;
            Body = body;
            Title = title;
        }

        public string Url { get; private set; }
        public string Body { get; private set; }
        public string Title { get; private set; }
    }
}