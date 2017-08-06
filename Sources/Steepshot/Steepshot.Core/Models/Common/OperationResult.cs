using System.Collections.Generic;
using System.Linq;

namespace Steepshot.Core.Models.Common
{
    public class OperationResult
    {
        public bool Success => !Errors.Any();
        public List<string> Errors { get; set; }

        public OperationResult()
        {
            Errors = new List<string>();
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Result { get; set; }
    }
}