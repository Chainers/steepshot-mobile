using Steepshot.Core.Authority;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UploadMediaModel : AuthorizedModel
    {
        [Required(ErrorMessage = Localization.Errors.EmptyFileField)]
        public Stream File { get; }

        public string ContentType { get; }

        public string VerifyTransaction { get; set; }

        public bool GenerateThumbnail { get; set; } = true;

        public UploadMediaModel(UserInfo user, Stream file, string extension)
            : base(user)
        {
            File = file;
            ContentType = MimeTypeHelper.GetMimeType(extension);
        }
    }
}
