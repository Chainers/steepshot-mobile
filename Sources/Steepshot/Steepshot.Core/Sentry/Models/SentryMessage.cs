using System;
using System.Linq;
using Newtonsoft.Json;

namespace Steepshot.Core.Sentry.Models
{
    /// <summary>
    /// Captures a <see cref="string"/> message, optionally formatted with arguments,
    /// as sent to Sentry.
    /// </summary>
    public class SentryMessage
    {
        private readonly string message;
        private readonly object[] parameters;


        /// <summary>
        /// Initializes a new instance of the <see cref="SentryMessage"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="parameters">The arguments.</param>
        public SentryMessage(string format, params object[] parameters)
            : this(format)
        {
            this.parameters = parameters;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SentryMessage"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SentryMessage(string message)
        {
            this.message = message;
        }


        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        [JsonProperty(PropertyName = "params", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Parameters
        {
            get { return parameters; }
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (message != null && parameters != null && parameters.Any())
            {
                try
                {
                    return String.Format(message, parameters);
                }
                catch
                {
                    return message;
                }
            }

            return message ?? String.Empty;
        }

        #region Operators

        /// <summary>
        /// Implicitly converts the <see cref="string"/> <paramref name="message"/> to a <see cref="SentryMessage"/> object.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// The <see cref="string"/> <paramref name="message"/> implicitly converted to a <see cref="SentryMessage"/> object.
        /// </returns>
        public static implicit operator SentryMessage(string message)
        {
            return message != null
                ? new SentryMessage(message)
                : null;
        }


        /// <summary>
        /// Implicitly converts the <see cref="SentryMessage"/> object to a <see cref="string"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// The <see cref="SentryMessage"/> object implicitly converted to a <see cref="string"/>.
        /// </returns>
        public static implicit operator string(SentryMessage message)
        {
            return message != null
                ? message.ToString()
                : null;
        }

        #endregion
    }
}