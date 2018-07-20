using System;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    public sealed class InternalError : Exception
    {
        public LocalizationKeys Key;

        public override string Message => ToString();

        public InternalError(LocalizationKeys key, Exception ex) : base(key.ToString(), ex)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
