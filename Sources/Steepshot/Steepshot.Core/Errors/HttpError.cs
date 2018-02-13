using Ditch.Core.Errors;
using Newtonsoft.Json;

namespace Steepshot.Core.Errors
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class HttpError : ErrorBase
    {
        public SystemError SystemError { get; }

        public HttpError(SystemError systemError)
        {
            SystemError = systemError;
        }
    }
}