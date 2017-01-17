using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class CreateCommentRequest : SessionIdField
    {
        public CreateCommentRequest(string sessionId, string url, string body, string title) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
            Body = body;
            Title = title;
        }

        public string Url { get; private set; }
        public string Body { get; private set; }
        public string Title { get; private set; }
    }

    public class GetCommentsRequest
    {
        public GetCommentsRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
        }

        public string Url { get; private set; }
    }
}