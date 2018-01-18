using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DeleteModel : AuthorizedModel
    {
        public DeleteModel(UserInfo user, string url) : base(user)
        {
            Url = url;
        }

        [JsonProperty()]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string Url { get; set; }
    }
}
