using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest : SessionIdField
    {
        public PostsRequest(string sessionId, PostType type, int limit, string offset = "") : base(sessionId)
        {
            if (limit < 0)
            {
                throw new ArgumentException(nameof(limit));
            }

            Type = type;
            Limit = limit;
            Offset = offset;
        }

        public PostType Type { get; private set; }

        public int Limit { get; private set; }

        public string Offset { get; private set; }
    }
}