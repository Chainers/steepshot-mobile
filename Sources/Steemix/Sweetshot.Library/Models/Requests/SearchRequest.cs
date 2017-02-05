using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class SearchRequest : OffsetLimitFields
    {
        public SearchRequest(string query)
        {
            Query = query;
        }

        public string Query { get; private set; }
    }
}