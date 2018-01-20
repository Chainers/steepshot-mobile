using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DeleteModel : AuthorizedModel
    {
        public DeleteModel(string login, string postingKey, string url) : base(login, postingKey)
        {
            Url = url;
        }

        [JsonProperty()]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string Url { get; set; }
    }
}
