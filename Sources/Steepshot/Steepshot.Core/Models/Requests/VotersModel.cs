using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VotersModel : InfoModel
    {
        [JsonProperty]
        [Required]
        public VotersType Type { get; }


        public VotersModel(string url, VotersType type) : base(url)
        {
            Type = type;
        }
    }
}