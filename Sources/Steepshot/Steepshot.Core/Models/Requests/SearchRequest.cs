namespace Steepshot.Core.Models.Requests
{
    public class SearchRequest : SessionIdOffsetLimitFields
    {
    }

    public class SearchWithQueryRequest : SearchRequest
    {
        public SearchWithQueryRequest(string query)
        {
            Query = query;
        }

        public string Query { get; private set; }
    }
}