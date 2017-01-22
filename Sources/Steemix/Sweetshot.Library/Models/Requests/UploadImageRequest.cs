using System;
using System.Collections.Generic;

namespace Sweetshot.Library.Models.Requests
{
    public class UploadImageRequest
    {
        public UploadImageRequest(string sessionId, string title, byte[] photo, params string[] tags)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }
            if (photo == null)
            {
                throw new ArgumentNullException(nameof(photo));
            }

            SessionId = sessionId;
            Title = title;
            Photo = photo;
            Tags = new List<string>(tags);
        }

        public string SessionId { get; private set; }
        public string Title { get; private set; }
        public byte[] Photo { get; private set; }
        public List<string> Tags { get; private set; }
    }
}