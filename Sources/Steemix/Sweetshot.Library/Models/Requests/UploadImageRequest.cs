using System;
using System.Collections.Generic;

namespace Sweetshot.Library.Models.Requests
{
    public class UploadImageRequest
    {
        public UploadImageRequest(string sessionId, string title, params string[] tags)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            SessionId = sessionId;
            Title = title;
            Tags = new List<string>(tags);
        }

        public UploadImageRequest(string sessionId, string title, byte[] photo, params string[] tags) : this(sessionId, title, tags)
        {
            if (photo == null)
            {
                throw new ArgumentNullException(nameof(photo));
            }

            Photo = photo;
        }

        public UploadImageRequest(string sessionId, string title, string photo, params string[] tags) : this(sessionId, title, tags)
        {
            if (string.IsNullOrWhiteSpace(photo))
            {
                throw new ArgumentNullException(nameof(photo));
            }

            Photo = Convert.FromBase64String(photo);
        }

        public string SessionId { get; private set; }
        public string Title { get; private set; }
        public byte[] Photo { get; private set; }
        public List<string> Tags { get; private set; }
    }
}