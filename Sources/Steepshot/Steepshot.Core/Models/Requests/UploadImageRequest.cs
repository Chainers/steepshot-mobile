using System;

namespace Steepshot.Core.Models.Requests
{
    public class UploadImageRequest : BaseRequest
    {
        private UploadImageRequest(string sessionId, string title, params string[] tags)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentNullException(nameof(title));

            SessionId = sessionId;
            Title = title;
            Tags = tags;
        }

        public UploadImageRequest(string sessionId, string title, byte[] photo, params string[] tags) : this(sessionId, title, tags)
        {
            Photo = photo ?? throw new ArgumentNullException(nameof(photo));
        }

        public UploadImageRequest(string sessionId, string title, string photo, params string[] tags) : this(sessionId, title, tags)
        {
            if (string.IsNullOrWhiteSpace(photo)) throw new ArgumentNullException(nameof(photo));

            Photo = Convert.FromBase64String(photo);
        }

        public string Title { get; private set; }
        public byte[] Photo { get; private set; }
        public string[] Tags { get; private set; }
    }
}