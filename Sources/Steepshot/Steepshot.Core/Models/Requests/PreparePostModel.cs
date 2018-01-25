using Steepshot.Core.Models.Responses;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    public class PreparePostModel : AuthorizedModel
    {
        public const int TagLimit = 20;


        public string Description { get; set; }

        public string PostPermlink { get; set; }

        [MaxLength(TagLimit, ErrorMessage = Localization.Errors.TagLimitError)]
        public string[] Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BeneficiariesSet { get; }

        public bool ShowFooter { get; }

        [MinLength(1)]
        public UploadMediaResponse[] Media { get; set; }

        public string Title { get; set; }

        public PreparePostModel(UserInfo userInfo) : base(userInfo)
        {
            if (!userInfo.IsNeedRewards)
                BeneficiariesSet = "steepshot_no_rewards";

            ShowFooter = userInfo.ShowFooter;
        }
    }
}