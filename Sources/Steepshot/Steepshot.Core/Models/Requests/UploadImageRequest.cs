using System;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class UploadImageRequest : LoginRequest
    {
        private UploadImageRequest(UserInfo user, string title, params string[] tags)
            : base(user)
        {
            Title = title;
            Tags = tags;
        }

        public UploadImageRequest(UserInfo user, string title, byte[] photo, params string[] tags)
            : this(user, title, tags)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            Photo = photo;
        }

        public UploadImageRequest(UserInfo user, string title, string photo, params string[] tags)
            : this(user, title, tags)
        {
            if (string.IsNullOrWhiteSpace(photo)) throw new ArgumentNullException(nameof(photo));

            Photo = Convert.FromBase64String(photo);
        }

        public string Title { get; private set; }
        public byte[] Photo { get; private set; }
        public string[] Tags { get; private set; }
    }
}