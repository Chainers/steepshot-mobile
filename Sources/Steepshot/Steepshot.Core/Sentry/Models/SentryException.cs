using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Steepshot.Core.Sentry.Models
{
    /// <summary>
    /// Represents Sentry's version of an <see cref="Exception"/>.
    /// </summary>
    public class SentryException
    {
        /// <summary>
        /// The module where the exception happened.
        /// </summary>
        [JsonProperty(PropertyName = "module")]
        public string Module { get; set; }

        /// <summary>
        /// The stacktrace of the exception.
        /// </summary>
        [JsonProperty(PropertyName = "stacktrace")]
        public SentryStacktrace Stacktrace { get; set; }

        /// <summary>
        /// The type of exception.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// The message of the exception.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        private readonly string _message;


        /// <summary>
        /// Initializes a new instance of the <see cref="SentryException"/> class.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        public SentryException(Exception exception)
        {
            if (exception == null)
                return;

            _message = exception.Message;
            Module = exception.Source;
            Type = exception.GetType().FullName;
            Value = exception.Message;

            Stacktrace = new SentryStacktrace(exception);
            if (Stacktrace.Frames == null || Stacktrace.Frames.Length == 0)
                Stacktrace = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Type != null)
                sb.Append(Type);

            if (_message != null)
            {
                if (sb.Length > 0)
                    sb.Append(": ");

                sb.Append(_message);
                sb.AppendLine();
            }

            if (Stacktrace != null)
                sb.Append(Stacktrace);

            return sb.ToString().TrimEnd();
        }

        public static List<SentryException> GetList(Exception exception)
        {
            var exceptions = new List<SentryException>();

            for (var currentException = exception;
                currentException != null;
                currentException = currentException.InnerException)
            {
                var sentryException = new SentryException(currentException);

                exceptions.Add(sentryException);
            }

            // ReflectionTypeLoadException doesn't contain much useful info in itself, and needs special handling
            var reflectionTypeLoadException = exception as ReflectionTypeLoadException;
            if (reflectionTypeLoadException != null)
            {
                foreach (var loaderException in reflectionTypeLoadException.LoaderExceptions)
                {
                    var sentryException = new SentryException(loaderException);

                    exceptions.Add(sentryException);
                }
            }
            return exceptions;
        }
    }
}