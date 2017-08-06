using System;

namespace Steepshot.Core.Models.Requests
{
    public class CreateCommentRequest : BaseRequest
    {
        public CreateCommentRequest(string sessionId, string url, string body, string title)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            base.SessionId = sessionId;
            Url = url;
            Body = body;
            Title = title;
        }

        public string Url { get; private set; }
        public string Body { get; private set; }
        public string Title { get; private set; }
    }
}