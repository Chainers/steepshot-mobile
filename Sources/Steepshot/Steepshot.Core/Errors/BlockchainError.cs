using System.Linq;
using Ditch.Core.Errors;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Errors
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class BlockchainError : ErrorBase
    {
        public BlockchainError(LocalizationKeys key) : base(key) { }

        public BlockchainError(ResponseError responseError)
            : base(ToMessage(responseError)) { }

        private static string ToMessage(ResponseError responseError)
        {
            var format = string.Empty;
            if (responseError.Data.Stack.Any() && !string.IsNullOrEmpty(responseError.Data.Stack[0].Format))
                format = ": " + responseError.Data.Stack[0].Format;

            return $"{responseError.Data.Code} {responseError.Data.Name}: {responseError.Data.Message}{format}";
        }
    }
}