using System;

namespace Sweetshot.Library.Models.Requests
{
    public class CreateCommentsRequest : GetCommentsRequest
    {
        public CreateCommentsRequest(string sessionId, string url, string body, string title) : base(sessionId, url)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentNullException(nameof(body));
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            Body = body;
            Title = title;
        }

        public string Body { get; private set; }
        public string Title { get; private set; }
    }
}