using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Steepshot.Core.Errors
{
    public sealed class ValidationError : ErrorBase
    {
        public ValidationError(List<ValidationResult> results) : base(results.FirstOrDefault()?.ErrorMessage)
        {
        }
    }
}