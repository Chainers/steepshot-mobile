using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class CreateCommentsRequest : GetCommentsRequest
    {
        public CreateCommentsRequest(string sessionId, string url, string body, string title = "") : base(sessionId, url)
        {
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentNullException(nameof(body));

            Body = body;
            Title = title;
        }

        public string Body { get; private set; }
        public string Title { get; private set; }
    }

    public class GetCommentsRequest : SessionIdField
    {
        public GetCommentsRequest(string sessionId, string url) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public string Url { get; private set; }
    }
}