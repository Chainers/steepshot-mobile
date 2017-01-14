using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    public class UserPostResponse
    {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public List<UserPost> Results { get; set; }
    }

    public class UserPost
    {
        public string Body { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public string Avatar { get; set; }
        public string AuthorRewards { get; set; }
        public string AuthorReputation { get; set; }
        public string NetVotes { get; set; }
        public string Children { get; set; }
        public string Created { get; set; }
        public string CuratorPayoutValue { get; set; }
        public string TotalPayoutValue { get; set; }
        public string PendingPayoutValue { get; set; }
        public List<string> Replies { get; set; }
        public bool Vote { get; set; }
    }
}