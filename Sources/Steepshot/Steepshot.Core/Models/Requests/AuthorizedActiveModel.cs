using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AuthorizedActiveModel
    {
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Login { get; }
        
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyActiveKey))]
        public string ActiveKey { get; set; }
        

        public AuthorizedActiveModel(string login, string activeKey)
        {
            Login = login;
            ActiveKey = activeKey;
        }
    }
}
