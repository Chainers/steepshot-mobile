using System;
namespace Steepshot.Core.Models.Responses
{
    public class SpamResponse
    {
        public int CountPostsLastDay { get; set; }
        public bool IsSpam { get; set; }
        public float WaitingTime { get; set; }
    }
}
