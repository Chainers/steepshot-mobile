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

    // TODO Offset and Limit ?
    public class GetCommentsRequest : UrlField
    {
        public GetCommentsRequest(string url) : base(url)
        {
        }
    }
}