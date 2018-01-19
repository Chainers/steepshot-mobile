using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PostsByCategoryModel : PostsModel
    {
        [JsonProperty]
        [Required]
        public string Category { get; set; }


        public PostsByCategoryModel(PostType type, string category) : base(type)
        {
            Category = category;
        }
    }
}