using System;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Exceptions
{
    public sealed class ValidationException : Exception
    {
        public object[] Parameters { get; set; }
        public LocalizationKeys? Key { get; set; }

        public override string Message => ToString();


        public ValidationException(string message)
            : base(message)
        {
        }

        public ValidationException(LocalizationKeys key, params object[] parameters)
        {
            Key = key;
            Parameters = parameters;
        }

        public ValidationException(LocalizationKeys key)
        {
            Key = key;
        }

        public override string ToString()
        {
            if (Parameters == null)
                return Key.ToString();

            return string.Format($"{Key} {string.Join(",", Parameters)}");
        }
    }
}