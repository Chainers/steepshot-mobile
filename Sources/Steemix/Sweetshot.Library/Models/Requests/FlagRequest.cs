using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public enum FlagType
    {
        [Description("flag")] Up,
        [Description("noflag")] Down
    }

    public class FlagRequest : SessionIdField
    {
        public FlagRequest(string sessionId, bool isUp, string identifier)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            base.SessionId = sessionId;
            Type = isUp ? FlagType.Up : FlagType.Down;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [JsonIgnore]
        public FlagType Type { get; private set; }
    }
}