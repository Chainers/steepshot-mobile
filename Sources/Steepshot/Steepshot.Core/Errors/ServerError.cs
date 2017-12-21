using Newtonsoft.Json;

namespace Steepshot.Core.Errors
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerError : ErrorBase
    {
        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="message">ResponseError message</param>
        public ServerError(string message) : base(message) { }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="code">ResponseError code</param>
        /// <param name="message">ResponseError message</param>
        public ServerError(long code, string message) : base(code, message) { }
    }
}