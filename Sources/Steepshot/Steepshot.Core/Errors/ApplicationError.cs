namespace Steepshot.Core.Errors
{
    public class ApplicationError : ErrorBase
    {
        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="message">ResponseError message</param>
        public ApplicationError(string message) : base(message) { }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="code">ResponseError code</param>
        /// <param name="message">ResponseError message</param>
        public ApplicationError(long code, string message) : base(code, message) { }
    }
}