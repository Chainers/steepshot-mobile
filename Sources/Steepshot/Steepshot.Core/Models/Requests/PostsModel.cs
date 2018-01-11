using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PostsModel : CensoredNamedRequestWithOffsetLimitModel
    {
        public PostsModel(PostType type)
        {
            Type = type;
        }

        [JsonProperty()]
        [Required()]
        public PostType Type { get; }
    }
}
