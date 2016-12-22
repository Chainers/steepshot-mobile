using System;

namespace Steemix.Library.Exceptions
{
    public class ApiGatewayException : Exception
    {
		public string ResponseContent { get; set; }
		
		public ApiGatewayException(string message)
            : base(message)
        {
        }

        public ApiGatewayException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        public ApiGatewayException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ApiGatewayException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }
    }
}