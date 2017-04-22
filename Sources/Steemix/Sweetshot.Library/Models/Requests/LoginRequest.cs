using System;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public interface ILoginRequest
    {
        string Username { get; set; }
    }

    public class LoginRequest : ILoginRequest
    {
        public LoginRequest(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            Username = username;
            Password = password;
        }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }

    public class RegisterRequest : LoginRequest
    {
        public RegisterRequest(string postingKey, string username, string password) : base(username, password)
        {
            PostingKey = postingKey;
        }

        [JsonProperty(PropertyName = "posting_key")]
        public string PostingKey { get; set; }
    }

    public class LoginWithPostingKeyRequest : ILoginRequest
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