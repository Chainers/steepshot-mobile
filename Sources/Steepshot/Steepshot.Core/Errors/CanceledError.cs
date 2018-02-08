using Newtonsoft.Json;

namespace Steepshot.Core.Errors
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CanceledError : ErrorBase
    {
        public CanceledError() { }
    }
}