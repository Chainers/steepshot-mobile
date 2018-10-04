using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UploadMediaModel : AuthorizedTokenModel
    {
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyFileField))]
        public Stream File { get; }

        public string ContentType { get; set; }

        public bool Thumbnails { get; set; } = true;

        public bool AWS { get; set; } = true;

        public bool IPFS { get; set; } = true;

        public UploadMediaModel(UserInfo user, Stream file, string extension)
            : base(user)
        {
            File = file;
            ContentType = MimeTypeHelper.GetMimeType(extension);
        }

        public UploadMediaModel(UserInfo user, Stream file)
            : base(user)
        {
            File = file;
        }
    }
}
