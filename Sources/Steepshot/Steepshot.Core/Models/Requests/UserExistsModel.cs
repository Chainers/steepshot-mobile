using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserExistsModel
    {
        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }


        public UserExistsModel(string username)
        {
            Username = username;
        }
    }
}