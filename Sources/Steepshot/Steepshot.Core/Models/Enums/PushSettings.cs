using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Steepshot.Core.Models.Enums
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PushSettings
    {
        None = 0,

        [EnumMember(Value = "upvote")]
        Upvote = 1,

        [EnumMember(Value = "upvote_comment")]
        UpvoteComment = 2,

        [EnumMember(Value = "comment")]
        Comment = 4,

        [EnumMember(Value = "follow")]
        Follow = 8,

        [EnumMember(Value = "post")]
        User = 16,

        All = Upvote | UpvoteComment | Comment | Follow | User

    }
}
