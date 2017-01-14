using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public enum VoteType
    {
        [Description("upvote")] Up,
        [Description("downvote")] Down
    }

    public class VoteRequest : SessionIdField
    {
        public VoteRequest(string sessionId, bool isUpVote, string identifier) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));

            Type = isUpVote ? VoteType.Up : VoteType.Down;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        public VoteType Type { get; private set; }
    }
}