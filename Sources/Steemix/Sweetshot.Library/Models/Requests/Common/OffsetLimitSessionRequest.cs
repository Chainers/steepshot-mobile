using System;

namespace Sweetshot.Library.Models.Requests.Common
{
    public class OffsetLimitSessionRequest : SessionIdField
    {
        public OffsetLimitSessionRequest(string sessionId, string offset = "", int limit = 0) : base(sessionId)
        {
            if (limit < 0)
            {
                throw new ArgumentException("Limit must be positive number.", nameof(limit));
            }
            Offset = offset;
            Limit = limit;
        }

        public string Offset { get; private set; }
        public int Limit { get; private set; }
    }
}