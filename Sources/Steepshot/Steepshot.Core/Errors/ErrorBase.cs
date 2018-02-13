using Steepshot.Core.Localization;
using System;

namespace Steepshot.Core.Errors
{
    [Serializable]
    public abstract class ErrorBase : Exception
    {
        public ErrorBase()
        {
        }

        protected ErrorBase(string key)
            : base(key) { }

        public ErrorBase(LocalizationKeys key)
            : base(key.ToString()) { }
    }
}