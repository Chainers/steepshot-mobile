using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class LogoutRequest : SessionIdField
    {
        public LogoutRequest(string sessionId) : base(sessionId)
        {
        }
    }
}