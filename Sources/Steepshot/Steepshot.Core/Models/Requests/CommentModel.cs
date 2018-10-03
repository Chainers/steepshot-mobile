using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CommentModel : AuthorizedWifModel
    {
        [JsonProperty]
        public string ParentAuthor { get; }

        [JsonProperty]
        public string ParentPermlink { get; }

        [JsonProperty]
        public string Author { get; set; }

        [JsonProperty]
        public string Permlink { get; }

        [JsonProperty]
        public string Title { get; }

        [JsonProperty]
        public string Body { get; }

        [JsonProperty]
        public string JsonMetadata { get; }

        [JsonProperty]
        public Beneficiary[] Beneficiaries { get; set; }


        public CommentModel(string login, string postingKey, string parentAuthor, string parentPermlink, string author, string permlink, string title, string body, string jsonMetadata)
            : base(login, postingKey)
        {
            ParentAuthor = parentAuthor;
            ParentPermlink = parentPermlink;
            Author = author;
            Permlink = permlink;
            Title = title;
            Body = body;
            JsonMetadata = jsonMetadata;
        }

        public CommentModel(UserInfo user, Post parentPost, string permlink, string title, string body, string jsonMetadata)
            : base(user)
        {
            ParentAuthor = parentPost.Author;
            ParentPermlink = parentPost.Permlink;
            Author = user.Login;
            Permlink = permlink;
            Title = title;
            Body = body;
            JsonMetadata = jsonMetadata;
        }

        public CommentModel(PreparePostModel model, string body, string meta)
            : this(model.Login, model.PostingKey, string.Empty, model.Category, model.Login, model.Permlink, model.Title, body, meta)
        {
        }
    }
}
