using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class CategoriesRequest : OffsetLimitFields
    {
        public CategoriesRequest(string offset = "", int limit = 0) : base(offset, limit)
        {
        }
    }

    public class SearchCategoriesRequest : CategoriesRequest
    {
        public SearchCategoriesRequest(string query, string offset = "", int limit = 0) : base(offset, limit)
        {
            Query = query;
        }

        public string Query { get; private set; }
    }
}