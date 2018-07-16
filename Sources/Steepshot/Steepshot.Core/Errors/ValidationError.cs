using System;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    public sealed class ValidationError : Exception
    {
        public object[] Parameters { get; set; }
        public LocalizationKeys? Key { get; set; }

        public ValidationError(string message)
            : base(message)
        {
        }

        public ValidationError(LocalizationKeys key, params object[] parameters)
        {
            Key = key;
            Parameters = parameters;
        }

        public ValidationError(LocalizationKeys key)
        {
            Key = key;
        }
    }
}