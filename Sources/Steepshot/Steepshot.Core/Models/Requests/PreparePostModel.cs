using Newtonsoft.Json;
using Steepshot.Core.Authority;
using System.ComponentModel.DataAnnotations;
using Ditch.Core.Helpers;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PreparePostModel : AuthorizedModel
    {
        private string[] _tags;
        private string _postPermlink;
        public const int TagLimit = 20;

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string PostPermlink
        {
            get { return string.IsNullOrEmpty(_postPermlink) ? OperationHelper.TitleToPermlink(Title) : _postPermlink; }
            set { _postPermlink = value; }
        }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyLogin)]
        public string Username { get; set; }

        [JsonProperty]
        [MaxLength(TagLimit, ErrorMessage = Localization.Errors.TagLimitError)]
        public string[] Tags
        {
            get { return _tags; }
            set
            {
                if (value != null)
                    OperationHelper.PrepareTags(value);
                _tags = value;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BeneficiariesSet { get; }

        [JsonProperty]
        public bool ShowFooter { get; }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyFileField)]
        public MediaModel[] Media { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyTitleField)]
        public string Title { get; set; }


        public PreparePostModel(UserInfo user) : base(user)
        {
            if (!user.IsNeedRewards)
                BeneficiariesSet = "steepshot_no_rewards";

            ShowFooter = user.ShowFooter;
            Username = user.Login;
        }
    }
}