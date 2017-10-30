using System;
using Steepshot.Core.Authority;
using Steepshot.Core.Exceptions;

namespace Steepshot.Core.Models.Requests
{
    public class UploadImageRequest : AuthorizedRequest
    {
        private UploadImageRequest(UserInfo user, string title, params string[] tags) : base(user)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new UserException(Localization.Errors.EmptyTitleField);

            Title = title;
            Tags = tags;
            IsNeedRewards = user.IsNeedRewards;
        }

        public UploadImageRequest(UserInfo user, string title, byte[] photo, params string[] tags) : this(user, title, tags)
        {
            if (photo == null || photo.Length == 0)
                throw new UserException(Localization.Errors.EmptyPhotoField);

            Photo = photo;
        }

        public UploadImageRequest(UserInfo user, string title, string photo, params string[] tags) : this(user, title, tags)
        {
            if (string.IsNullOrWhiteSpace(photo))
                throw new UserException(Localization.Errors.EmptyPhotoField);

            Photo = Convert.FromBase64String(photo);
        }

        public string Title { get; }

        public byte[] Photo { get; }

        public string Description { get; set; }

        public string[] Tags { get; }

        public bool IsNeedRewards { get; }
    }
}
