using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    public class DeleteRequest : AuthorizedRequest
    {
        public DeleteRequest(string login, string postingKey, string url) : base(login, postingKey)
        {
            Url = url;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string Url { get; set; }
    }
}
