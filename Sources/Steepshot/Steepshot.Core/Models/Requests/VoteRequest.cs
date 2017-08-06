using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    public enum VoteType
    {
        [Display(Description = "upvote")] Up,
        [Display(Description = "downvote")] Down
    }

    public class VoteRequest : BaseRequest
    {
        public VoteRequest(string sessionId, bool isUp, string identifier)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(identifier)) throw new ArgumentNullException(nameof(identifier));

            base.SessionId = sessionId;
            Type = isUp ? VoteType.Up : VoteType.Down;
            Identifier = identifier;
        }

        [JsonProperty(PropertyName = "identifier")]
        public string Identifier { get; private set; }

        [JsonIgnore]
        public VoteType Type { get; private set; }
    }
}