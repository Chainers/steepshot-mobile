using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PostsByCategoryModel : PostsModel
    {
        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyCategory))]
        public string Category { get; set; }


        public PostsByCategoryModel(PostType type, string category) : base(type)
        {
            Category = category;
        }
    }
}