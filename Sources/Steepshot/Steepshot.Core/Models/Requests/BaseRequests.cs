using System;

namespace Steepshot.Core.Models.Requests
{
    public class SessionIdField
    {
        public string SessionId { get; set; }
    }

    public class OffsetLimitFields
    {
        public string Offset { get; set; }
        public int Limit { get; set; }
    }

    public class SessionIdOffsetLimitFields : OffsetLimitFields
    {
        public string SessionId { get; set; }
    }
    
    public class InfoRequest : SessionIdField
    {
        public InfoRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public string Url { get; private set; }
    }
}