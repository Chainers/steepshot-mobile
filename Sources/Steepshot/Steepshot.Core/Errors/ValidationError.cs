using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    public sealed class ValidationError : ErrorBase
    {
        public ValidationError(List<ValidationResult> results)
            : base(results.FirstOrDefault()?.ErrorMessage)
        {
        }

        public ValidationError(LocalizationKeys key, params object[] parameters)
            : base(key, parameters)
        {
        }

        public ValidationError(LocalizationKeys key)
            : base(key)
        {
        }

        public ValidationError()
            : base(string.Empty)
        {
        }
    }
}