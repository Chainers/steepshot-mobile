using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserExistsModel
    {
        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Username { get; }


        public UserExistsModel(string username)
        {
            Username = username;
        }
    }
}