using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public enum VoteType
    {
        [Description("upvote")] Up,
        [Description("downvote")] Down
    }

    public class VoteRequest : SessionIdField
    {
        public VoteRequest(string sessionId, bool isUpVote, string identifier)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            base.SessionId = sessionId;
            Type = isUpVote ? VoteType.Up : VoteType.Down;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }
        
        [JsonIgnore]
        public VoteType Type { get; private set; }
    }
}