using System;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    public sealed class InternalError : Exception
    {
        public LocalizationKeys Key;

        public InternalError(LocalizationKeys key, Exception ex) : base(key.ToString(), ex)
        {
            Key = key;
        }
    }
}
