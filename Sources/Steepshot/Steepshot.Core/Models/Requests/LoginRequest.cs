using System;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class LoginWithPostingKeyRequest
    {
        public LoginWithPostingKeyRequest(UserInfo user)
            : this(user.Login, user.PostingKey) { }

        public LoginWithPostingKeyRequest(string login, string postingKey)
        {
            if (string.IsNullOrWhiteSpace(login)) throw new ArgumentNullException(nameof(login));
            if (string.IsNullOrWhiteSpace(postingKey)) throw new ArgumentNullException(nameof(postingKey));

            Login = login;
            PostingKey = postingKey;
        }

        [JsonProperty(PropertyName = "username")]
        public string Login { get; set; }

        [JsonProperty(PropertyName = "posting_key")]
        public string PostingKey { get; set; }
    }
}