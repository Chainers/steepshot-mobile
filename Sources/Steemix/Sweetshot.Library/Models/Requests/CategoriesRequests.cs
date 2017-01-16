using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class CategoriesRequest : OffsetLimitSessionRequest
    {
        public CategoriesRequest(string sessionId, string offset = "", int limit = 0) : base(sessionId, offset, limit)
        {
        }
    }

    public class SearchCategoriesRequest : SessionIdField
    {
        public SearchCategoriesRequest(string sessionId, string query) : base(sessionId)
        {
            Query = query;
        }

        public string Query { get; private set; }
    }
}