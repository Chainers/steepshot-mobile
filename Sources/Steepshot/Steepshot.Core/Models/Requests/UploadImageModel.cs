using System;
using Steepshot.Core.Authority;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ditch.Core.Helpers;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UploadImageModel : AuthorizedModel
    {
        public const int TagLimit = 20;
        
        [Required(ErrorMessage = Localization.Errors.EmptyTitleField)]
        public string Title { get; }

        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string PostUrl { get; }

        [Required(ErrorMessage = Localization.Errors.EmptyPhotoField)]
        [MinLength(1, ErrorMessage = Localization.Errors.EmptyPhotoField)]
        public byte[] Photo { get; }

        public string Description { get; set; }

        [MaxLength(TagLimit, ErrorMessage = Localization.Errors.TagLimitError)]
        public string[] Tags { get; }

        public bool IsNeedRewards { get; }

        public string VerifyTransaction { get; set; }
        
        private UploadImageModel(UserInfo user, string title, IList<string> tags) : base(user)
        {
            Title = title;
            Tags = tags.Any() ? tags.Select(i => i.ToLower()).Distinct().ToArray() : new string[0];
            IsNeedRewards = user.IsNeedRewards;
            PostUrl = OperationHelper.TitleToPermlink(title);
        }

        public UploadImageModel(UserInfo user, string title, byte[] photo, IList<string> tags) : this(user, title, tags)
        {
            Photo = photo;
        }

        public UploadImageModel(UserInfo user, string title, string photo, IList<string> tags) : this(user, title, tags)
        {
            Photo = Convert.FromBase64String(photo);
        }

    }
}
