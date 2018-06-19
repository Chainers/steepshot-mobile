using Steepshot.Core.Localization;
using System;

namespace Steepshot.Core.Errors
{
    [Serializable]
    public abstract class ErrorBase : Exception
    {
        public object[] Parameters { get; set; }

        public ErrorBase()
        {
        }

        protected ErrorBase(string key) : base(key) { }

        protected ErrorBase(string key, object[] parameters) : base(key)
        {
            Parameters = parameters;
        }

        public ErrorBase(LocalizationKeys key) : base(key.ToString()) { }

        public ErrorBase(LocalizationKeys key, object[] parameters) : base(key.ToString())
        {
            Parameters = parameters;
        }
    }
}