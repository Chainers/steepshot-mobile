using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Steepshot.Core.Sentry.Models
{
    /// <summary>
    /// Represents Sentry's version of an <see cref="Exception"/>'s stack trace.
    /// </summary>
    public class SentryStacktrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SentryStacktrace"/> class.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        public SentryStacktrace(Exception exception)
        {
            if (exception == null)
                return;

            var trace = new StackFrame(exception, true);
            StackFrame[] frames = trace.GetFrames();

            if (frames == null)
                return;

            // Sentry expects the frames to be sent in reversed order
            Frames = frames.Reverse().Select(f => new ExceptionFrame(f)).ToArray();
        }


        /// <summary>
        /// Gets or sets the <see cref="Exception"/> frames.
        /// </summary>
        /// <value>
        /// The <see cref="Exception"/> frames.
        /// </value>
        [JsonProperty(PropertyName = "frames")]
        public ExceptionFrame[] Frames { get; set; }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (Frames == null || !Frames.Any())
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            // Have to reverse the frames before presenting them 
            // since they are stored in reversed order.
            foreach (var frame in Frames.Reverse())
            {
                sb.Append("   at ");
                sb.Append(frame);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}