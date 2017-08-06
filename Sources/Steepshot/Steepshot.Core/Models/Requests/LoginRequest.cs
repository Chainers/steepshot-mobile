using System;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    public class LoginWithPostingKeyRequest : BaseRequest
    {
        public LoginWithPostingKeyRequest(string username, string postingKey)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(postingKey)) throw new ArgumentNullException(nameof(postingKey));

            Username = username;
            PostingKey = postingKey;
        }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "posting_key")]
        public string PostingKey { get; set; }
    }
}