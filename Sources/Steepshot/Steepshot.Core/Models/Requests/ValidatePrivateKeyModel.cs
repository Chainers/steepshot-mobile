using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ValidatePrivateKeyModel
    {
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Login { get; }

        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyPostingKey))]
        public string PrivateKey { get; }

        public KeyRoleType KeyRoleType { get; }


        public ValidatePrivateKeyModel(string login, string privateKey, KeyRoleType keyRoleType)
        {
            Login = login;
            PrivateKey = privateKey;
            KeyRoleType = keyRoleType;
        }
    }
}
