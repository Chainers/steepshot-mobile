using Newtonsoft.Json;

namespace Steepshot.Core.Errors
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class TaskCanceledError : ErrorBase
    {
        public TaskCanceledError() : base(string.Empty) { }
    }
}