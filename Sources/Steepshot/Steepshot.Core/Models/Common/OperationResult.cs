using System;

namespace Steepshot.Core.Models.Common
{
    public class OperationResult
    {
        public bool IsSuccess => Error == null;

        public Exception Error { get; set; }

        public OperationResult()
        {
        }

        public OperationResult(Exception error)
        {
            Error = error;
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Result { get; set; }

        public OperationResult() { }

        public OperationResult(Exception error) : base(error) { }

        public OperationResult(T result)
        {
            Result = result;
        }
    }
}
