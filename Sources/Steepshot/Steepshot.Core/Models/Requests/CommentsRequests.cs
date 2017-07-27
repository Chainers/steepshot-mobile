using System;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class CreateCommentRequest : LoginRequest
    {
        public CreateCommentRequest(UserInfo user, string url, string body, string title)
            : base(user)
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

    public class GetCommentsRequest : LoginOffsetLimitFields
    {
        public string Url { get; private set; }

        public GetCommentsRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public GetCommentsRequest(string url, UserInfo user) : base(user)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
        }
    }

    public class GetVotesRequest : GetCommentsRequest
    {
        public GetVotesRequest(string url) : base(url)
        {

        }
        public GetVotesRequest(string url, UserInfo user)
            : base(url, user)
        {

        }
    }
}