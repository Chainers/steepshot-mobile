using System;
using System.Xml.Xsl;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public class IsLowRatedRequest : SessionIdField
    {
        public IsLowRatedRequest(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            base.SessionId = sessionId;
        }
    }

    public class SetLowRatedRequest : IsLowRatedRequest
    {
        public SetLowRatedRequest(string sessionId, bool showLowRated) : base(sessionId)
        {
            ShowLowRated = showLowRated;
        }

        [JsonProperty(PropertyName = "show_low_rated")]
        public bool ShowLowRated { get; private set; }
    }
}