using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "title": "cat",
    ///  "tags": [
    ///    "cat1",
    ///    "cat2",
    ///    "cat3",
    ///    "cat4",
    ///    "steepshot"
    ///  ],
    ///  "body": "http://res.cloudinary.com/steepshot2/image/upload/v1484584571/bxfru7z88aeqkvermoiw.jpg"
    ///}
    public class ImageUploadResponse
    {
        public string Title { get; set; }
        public List<string> Tags { get; set; }
        public string Body { get; set; }
    }
}