namespace Steepshot.Core.Models.Requests
{
    public class SearchWithQueryRequest : OffsetLimitFields
    {
        public SearchWithQueryRequest(string query)
        {
            Query = query;
        }

        public string Query { get; set; }

        public string Login { get; set; }
    }
}
