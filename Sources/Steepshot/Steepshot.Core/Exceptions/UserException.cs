using System;

namespace Steepshot.Core.Exceptions
{
    public class UserException : ApplicationExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"></see> class.
        /// </summary>
        public UserException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"></see> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public UserException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"></see> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public UserException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"></see> class with a specified error message.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="message">The message that describes the error. </param>
        public UserException(string paramName, string message) : base(paramName, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"></see> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public UserException(string paramName, string message, Exception innerException) : base(paramName, message, innerException)
        {
        }
    }
}