using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    public sealed class AppError : ErrorBase
    {
        public AppError(LocalizationKeys key) : base(key) { }
    }
}