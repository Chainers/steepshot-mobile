using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class SearchRequest : LoginOffsetLimitFields
    {
        public SearchRequest() { }

        public SearchRequest(UserInfo user) : base(user) { }
    }

    public class SearchWithQueryRequest : SearchRequest
    {
        public string Query { get; private set; }

        public SearchWithQueryRequest(string query)
        {
            Query = query;
        }

        public SearchWithQueryRequest(string query, UserInfo user) : base(user)
        {
            Query = query;
        }
    }
}