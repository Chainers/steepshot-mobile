using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Steepshot.Core.Models.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PushSubscription
    {
        [EnumMember(Value = "upvote")]
        Upvote,
        [EnumMember(Value = "comment_upvote")]
        CommentUpvote,
        [EnumMember(Value = "comment")]
        Comment,
        [EnumMember(Value = "follow")]
        Follow
    }
}
