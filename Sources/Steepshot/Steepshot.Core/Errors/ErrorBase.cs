using System;

namespace Steepshot.Core.Errors
{
    /// <summary>
    /// Iformation about error
    /// </summary>
    public class ErrorBase
    {
        /// <summary>
        /// ResponseError message
        /// </summary>
        public string Message { get; set; }
        
        public long Code { get; set; }


        public override string ToString()
        {
            return $"{base.ToString()}. Code: '{Code}'. Message: '{Message}'";
        }


        /// <summary>
        /// Default constructor of class
        /// </summary>
        protected ErrorBase()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="message">ResponseError message</param>
        protected ErrorBase(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="code">ResponseError code</param>
        /// <param name="message">ResponseError message</param>
        protected ErrorBase(long code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}