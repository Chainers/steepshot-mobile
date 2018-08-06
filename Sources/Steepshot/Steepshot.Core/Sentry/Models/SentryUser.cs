﻿using Newtonsoft.Json;

namespace Steepshot.Core.Sentry.Models
{
    /// <summary>
    /// An interface which describes the authenticated User for a request.
    /// You should provide at least either an id (a unique identifier for an authenticated user) or ip_address (their IP address).
    /// </summary>
    public class SentryUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SentryUser"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        public SentryUser(string username)
        {
            Username = username;
        }


        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        /// <value>
        /// The user's email address.
        /// </value>
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user's IP address.
        /// </summary>
        /// <value>
        /// The user's IP address.
        /// </value>
        [JsonProperty(PropertyName = "ip_address", NullValueHandling = NullValueHandling.Ignore)]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the user's username.
        /// </summary>
        /// <value>
        /// The user's username.
        /// </value>
        [JsonProperty(PropertyName = "username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }
    }
}