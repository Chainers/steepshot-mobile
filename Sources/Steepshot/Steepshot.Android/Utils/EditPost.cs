using System;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils
{
    [Serializable]
    class EditPost
    {
        public string[] Photos { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public string[] Tags { get; set; }

        public EditPost(Post post)
        {
            Photos = post.Photos;
            Description = post.Description;
            Title = post.Title;
            Url = post.Url;
            Category = post.Category;
            Author = post.Author;
            Tags = post.Tags;
        }
    }
}