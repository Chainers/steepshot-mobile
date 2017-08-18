using System;

namespace Steepshot.Core.Models.Requests
{
    public class NamedRequest
    {
        public string Login { get; set; }
    }

    public class OffsetLimitFields
    {
        public string Offset { get; set; } = string.Empty;
        public int Limit { get; set; } = 10;
    }

    public class NamedRequestWithOffsetLimitFields : NamedRequest
    {
        public string Offset { get; set; }
        public int Limit { get; set; }
    }

    public class InfoRequest : NamedRequestWithOffsetLimitFields
    {
        public InfoRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public string Url { get; }
    }
}