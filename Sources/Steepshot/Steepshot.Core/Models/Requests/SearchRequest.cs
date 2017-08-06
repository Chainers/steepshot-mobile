namespace Steepshot.Core.Models.Requests
{
    public class SearchRequest : BaseRequestWithOffsetLimitFields
    {
    }

    public class SearchWithQueryRequest : SearchRequest
    {
        public SearchWithQueryRequest(string query)
        {
            Query = query;
        }

        public string Query { get; set; }
    }
}