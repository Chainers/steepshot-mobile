using System;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public class CreateCommentRequest
    {
        public CreateCommentRequest(string sessionId, string url, string body, string title)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            SessionId = sessionId;
            Url = url;
            Body = body;
            Title = title;
        }

        [JsonIgnore]
        public string SessionId { get; private set; }
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