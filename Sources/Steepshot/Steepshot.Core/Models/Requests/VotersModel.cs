using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VotersModel : InfoModel
    {
        public VotersModel(string url, VotersType type) : base(url)
        {
            Type = type;
        }

        [JsonProperty()]
        [Required()]
        public VotersType Type { get; }
    }
}