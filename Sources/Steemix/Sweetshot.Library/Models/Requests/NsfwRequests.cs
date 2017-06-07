using System;
using System.Xml.Xsl;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public class IsNsfwRequest : SessionIdField
    {
        public IsNsfwRequest(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            base.SessionId = sessionId;
        }
    }

    public class SetNsfwRequest : IsNsfwRequest
    {
        public SetNsfwRequest(string sessionId, bool showNsfw) : base(sessionId)
        {
            ShowNsfw = showNsfw;
        }

        [JsonProperty(PropertyName = "show_nsfw")]
        public bool ShowNsfw { get; private set; }
    }
}