using System;
using Steepshot.Core.Authority;
using Steepshot.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Steepshot.Core.Models.Requests
{
    public class UploadImageRequest : AuthorizedRequest
    {
        private UploadImageRequest(UserInfo user, string title, IList<string> tags) : base(user)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new UserException("title", Localization.Errors.EmptyTitleField);

            Title = title;
            Tags = tags.Any() ? tags.Select(i => i.ToLower()).Distinct().ToArray() : new string[0];
            IsNeedRewards = user.IsNeedRewards;
        }

        public UploadImageRequest(UserInfo user, string title, byte[] photo, IList<string> tags) : this(user, title, tags)
        {
            if (photo == null || photo.Length == 0)
                throw new UserException("photo", Localization.Errors.EmptyPhotoField);

            Photo = photo;
        }

        public UploadImageRequest(UserInfo user, string title, string photo, IList<string> tags) : this(user, title, tags)
        {
            if (string.IsNullOrWhiteSpace(photo))
                throw new UserException("photo", Localization.Errors.EmptyPhotoField);

            Photo = Convert.FromBase64String(photo);
        }

        public string Title { get; }

        public byte[] Photo { get; }

        public string Description { get; set; }

        public string[] Tags { get; }

        public bool IsNeedRewards { get; }

        public string VerifyTransaction { get; set; }
    }
}
