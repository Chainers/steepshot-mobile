using Steepshot.Core.Errors;

namespace Steepshot.Core.Models.Common
{
    public class OperationResult
    {
        public bool IsSuccess => Error == null;

        public ErrorBase Error { get; set; }

        public OperationResult()
        {
        }

        public OperationResult(ErrorBase error)
        {
            Error = error;
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Result { get; set; }
        
        public OperationResult() { }

        public OperationResult(ErrorBase error) : base(error) { }

    }
}
