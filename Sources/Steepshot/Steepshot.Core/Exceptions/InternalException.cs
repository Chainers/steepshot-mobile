using System;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    public sealed class InternalException : Exception
    {
        public LocalizationKeys Key;

        public override string Message => ToString();

        public InternalException(LocalizationKeys key, Exception ex) : base(key.ToString(), ex)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
