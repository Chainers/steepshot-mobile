using System;

namespace Steepshot.Core.Models.Requests
{
    public class BaseRequest
    {
        public string SessionId { get; set; }
        public string Login { get; set; }
    }

    public class BaseRequestWithOffsetLimitFields : BaseRequest
    {
        public string Offset { get; set; }
        public int Limit { get; set; }
    }

    public class InfoRequest : BaseRequest
    {
        public InfoRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public string Url { get; private set; }
    }
}