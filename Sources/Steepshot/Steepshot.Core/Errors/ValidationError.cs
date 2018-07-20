using System;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    public sealed class ValidationError : Exception
    {
        public object[] Parameters { get; set; }
        public LocalizationKeys? Key { get; set; }

        public override string Message => ToString();


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

        public override string ToString()
        {
            if (Parameters == null)
                return Key.ToString();

            return string.Format($"{Key} {string.Join(",", Parameters)}");
        }
    }
}