using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Ditch.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using System.Diagnostics;
using System.Linq;
using Steepshot.Core.Authorization;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Requests
{
    [DebuggerStepThrough]
    [JsonObject(MemberSerialization.OptIn)]
    public class PreparePostModel : AuthorizedWifModel
    {
        private string[] _tags = new string[0];
        private string _permlink;
        private string _category;
        public const int TagLimit = 20;

        [JsonProperty]
        public string Description { get; set; } = string.Empty;

        public string Permlink
        {
            get
            {
                if (string.IsNullOrEmpty(_permlink) && !string.IsNullOrEmpty(Title))
                    _permlink = OperationHelper.TitleToPermlink(Title);

                return _permlink;
            }
        }

        //needed for post/prepare
        [JsonProperty]
        public string PostPermlink => $"@{Author}/{Permlink}";

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

        [JsonProperty]
        public bool ShowFooter { get; }

        private MediaModel[] _media;

        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyFileField))]
        public MediaModel[] Media
        {
            get
            {
                return _media;
            }
            set
            {
                foreach (var item in value)
                {
                    if (item != null && !string.IsNullOrEmpty(item.Url))
                        item.Url = item.Url.Replace("http:", "https:");
                }
                _media = value;
            }
        }

        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyTitleField))]
        public string Title { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyDeviceField))]
        public string Device { get; set; }

        public bool IsEditMode { get; }

        public string Category
        {
            get
            {
                if (string.IsNullOrEmpty(_category))
                    _category = Tags.Length > 0 ? Tags[0] : "steepshot";

                return _category;
            }
        }


        public PreparePostModel() { }

        public PreparePostModel(UserInfo user, string device) : base(user)
        {
            ShowFooter = user.ShowFooter;
            Author = user.Login;
            Device = device;
            IsEditMode = false;
        }

        public PreparePostModel(UserInfo user, Post post, string device) : base(user)
        {
            ShowFooter = user.ShowFooter;
            Author = user.Login;
            _permlink = post.Permlink;
            _category = post.Category;
            Device = device;
            IsEditMode = true;
        }
    }
}