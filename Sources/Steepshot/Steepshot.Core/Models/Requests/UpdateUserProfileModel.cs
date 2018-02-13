using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateUserProfileModel
    {
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyActiveKey))]
        public string ActiveKey { get; set; }

        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Login { get; set; }

        public string ProfileImage { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string Website { get; set; }

        public string About { get; set; }
    }
}