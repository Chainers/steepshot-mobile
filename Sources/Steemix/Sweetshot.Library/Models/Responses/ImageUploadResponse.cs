using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    public class ImageUploadResponse
    {
        public string Title { get; set; }
        public List<string> Tags { get; set; }
        public string Body { get; set; }
    }
}