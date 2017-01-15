using System;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public class RegisterRequest : LoginRequest
    {
        public RegisterRequest(string postingKey, string username, string password) : base(username, password)
        {
            if (string.IsNullOrWhiteSpace(postingKey))
                throw new ArgumentNullException(nameof(postingKey));

            PostingKey = postingKey;
        }

        [JsonProperty(PropertyName = "posting_key")]
        public string PostingKey { get; set; }
    }
}