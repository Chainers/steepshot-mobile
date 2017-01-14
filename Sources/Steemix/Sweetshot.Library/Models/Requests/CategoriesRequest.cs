using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class CategoriesRequest : SessionIdField
    {
        public CategoriesRequest(string sessionId, string offset = "") : base(sessionId)
        {
            Offset = offset;
        }

        public string Offset { get; private set; }
    }
}