using Newtonsoft.Json;
using Steepshot.Core.Authority;
using System.ComponentModel.DataAnnotations;
using Ditch.Core.Helpers;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PreparePostModel : AuthorizedModel
    {
        private string[] _tags;
        private readonly string _permlink;
        public const int TagLimit = 20;

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string Permlink => string.IsNullOrEmpty(_permlink) ? OperationHelper.TitleToPermlink(Title) : _permlink;

        [JsonProperty("username")]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyLogin))]
        public string Author { get; set; }

        [JsonProperty]
        [MaxLength(TagLimit, ErrorMessage = nameof(LocalizationKeys.TagLimitError))]
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
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyFileField))]

        public MediaModel[] Media { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyTitleField))]
        public string Title { get; set; }

        public bool IsEditMode { get; }

        public PreparePostModel(UserInfo user) : base(user)
        {
            if (!user.IsNeedRewards)
                BeneficiariesSet = "steepshot_no_rewards";

            ShowFooter = user.ShowFooter;
            Author = user.Login;
            IsEditMode = false;
        }

        public PreparePostModel(UserInfo user, string permlink) : base(user)
        {
            if (!user.IsNeedRewards)
                BeneficiariesSet = "steepshot_no_rewards";

            ShowFooter = user.ShowFooter;
            Author = user.Login;
            _permlink = permlink;
            IsEditMode = true;
        }
    }
}