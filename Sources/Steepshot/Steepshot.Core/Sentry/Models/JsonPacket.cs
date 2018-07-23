using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Steepshot.Core.Sentry.Models
{
    /// <summary>
    /// Represents the JSON packet that is transmitted to Sentry.
    /// </summary>
    public class JsonPacket
    {
        /// <summary>
        /// Function call which was the primary perpetrator of this event.
        /// A map or list of tags for this event.
        /// </summary>
        [JsonProperty(PropertyName = "culprit", NullValueHandling = NullValueHandling.Ignore)]
        public string Culprit { get; set; }

        /// <summary>
        /// Identifies the operating environment (e.g. production).
        /// </summary>
        [JsonProperty(PropertyName = "environment", NullValueHandling = NullValueHandling.Ignore)]
        public string Environment { get; set; } = string.Empty;

        /// <summary>
        /// Hexadecimal string representing a uuid4 value.
        /// </summary>
        [JsonProperty(PropertyName = "event_id", NullValueHandling = NullValueHandling.Ignore)]
        public string EventID { get; set; } = Guid.NewGuid().ToString("n");

        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        [JsonProperty(PropertyName = "exception", NullValueHandling = NullValueHandling.Ignore)]
        public List<SentryException> Exceptions { get; set; }

        /// <summary>
        /// An arbitrary mapping of additional metadata to store with the event.
        /// </summary>
        [JsonProperty(PropertyName = "extra", NullValueHandling = NullValueHandling.Ignore)]
        public object Extra { get; set; }

        /// <summary>
        /// Gets or sets the fingerprint used for custom grouping
        /// </summary>
        [JsonProperty(PropertyName = "fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Fingerprint { get; set; }

        /// <summary>
        /// The record severity.
        /// Defaults to error.
        /// </summary>
        [JsonProperty(PropertyName = "level", NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
        public string Level { get; set; }

        /// <summary>
        /// The name of the logger which created the record.
        /// If missing, defaults to the string root.
        ///
        /// Ex: "my.logger.name"
        /// </summary>
        [JsonProperty(PropertyName = "logger", NullValueHandling = NullValueHandling.Ignore)]
        public string Logger { get; set; } = "Steepshot.Mobile";

        /// <summary>
        /// User-readable representation of this event
        /// </summary>
        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public SentryMessage Message { get; set; }

        /// <summary>
        /// Optional Message with arguments.
        /// </summary>
        [JsonProperty(PropertyName = "sentry.interfaces.Message", NullValueHandling = NullValueHandling.Ignore)]
        public SentryMessage MessageObject { get; set; }

        /// <summary>
        /// A list of relevant modules (libraries) and their versions.
        /// Automated to report all modules currently loaded in project.
        /// </summary>
        /// <value>
        /// The modules.
        /// </value>
        [JsonProperty(PropertyName = "modules", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> Modules { get; set; }

        /// <summary>
        /// A string representing the platform the client is submitting from.
        /// This will be used by the Sentry interface to customize various components in the interface.
        /// </summary>
        [JsonProperty(PropertyName = "platform", NullValueHandling = NullValueHandling.Ignore)]
        public string Platform { get; set; } = "Xamarin";

        /// <summary>
        /// String value representing the project
        /// </summary>
        [JsonProperty(PropertyName = "project", NullValueHandling = NullValueHandling.Ignore)]
        public string Project { get; set; } = "default";

        /// <summary>
        /// Identifies the version of the application.
        /// </summary>
        [JsonProperty(PropertyName = "release", NullValueHandling = NullValueHandling.Ignore)]
        public string Release { get; set; } = string.Empty;

        /// <summary>
        /// Identifies the host client from which the event was recorded.
        /// </summary>
        [JsonProperty(PropertyName = "server_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ServerName { get; set; } = "Phone";

        /// <summary>
        /// A map or list of tags for this event.
        /// </summary>
        [JsonProperty(PropertyName = "tags", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> Tags { get; set; }

        ///// <summary>
        ///// Indicates when the logging record was created (in the Sentry client).
        ///// Defaults to DateTime.UtcNow()
        ///// </summary>
        //[JsonProperty(PropertyName = "timestamp", NullValueHandling = NullValueHandling.Ignore)]
        //public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SentryUser"/> object, which describes the authenticated User for a request.
        /// </summary>
        /// <value>
        /// The <see cref="SentryUser"/> object, which describes the authenticated User for a request.
        /// </value>
        [JsonProperty(PropertyName = "user", NullValueHandling = NullValueHandling.Ignore)]
        public SentryUser User { get; set; } = new SentryUser("unauthorized");



        /// <summary>
        /// Converts the <see cref="JsonPacket"/> into a JSON string.
        /// </summary>
        /// <returns>
        /// The <see cref="JsonPacket"/> as a JSON string.
        /// </returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}