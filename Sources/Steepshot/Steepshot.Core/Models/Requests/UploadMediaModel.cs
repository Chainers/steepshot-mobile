using Steepshot.Core.Authority;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UploadMediaModel : AuthorizedModel
    {
        [Required(ErrorMessage = Localization.Errors.EmptyFileField)]
        [MinLength(1, ErrorMessage = Localization.Errors.EmptyFileField)]
        public Stream File { get; }

        public string VerifyTransaction { get; set; }

        public bool GenerateThumbnail { get; set; } = true;

        public UploadMediaModel(UserInfo user, Stream file)
            : base(user)
        {
            File = file;
        }
    }
}
