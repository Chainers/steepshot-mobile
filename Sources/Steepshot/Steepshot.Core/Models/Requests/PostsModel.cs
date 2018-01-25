using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PostsModel : CensoredNamedRequestWithOffsetLimitModel
    {
        [JsonProperty]
        [Required]
        public PostType Type { get; }


        public PostsModel(PostType type)
        {
            Type = type;
        }
    }
}
