using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserExistsModel
    {
        public UserExistsModel(string username)
        {
            Username = username;
        }

        [JsonProperty()]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }
    }
}