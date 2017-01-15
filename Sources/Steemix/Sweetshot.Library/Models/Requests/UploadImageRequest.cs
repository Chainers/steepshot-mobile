using System;
using System.Collections.Generic;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class UploadImageRequest : SessionIdField
    {
        public UploadImageRequest(string sessionId, string title, byte[] photo, params string[] tags) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));

            Title = title;
            Photo = photo;

            if (tags.Length >= 1 && tags.Length <= 4)
                Tags = new List<string>(tags);
            else
                throw new ArgumentOutOfRangeException(nameof(tags), "The number of tags should be between 1 and 4.");
        }

        public string Title { get; private set; }
        public byte[] Photo { get; private set; }
        public List<string> Tags { get; private set; }
    }
}