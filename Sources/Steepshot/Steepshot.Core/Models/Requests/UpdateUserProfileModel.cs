using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateUserProfileModel
    {
        [Required(ErrorMessage = Localization.Errors.EmptyActiveKey)]
        public string ActiveKey { get; set; }

        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Login { get; set; }

        public string ProfileImage { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string Website { get; set; }

        public string About { get; set; }
    }
}