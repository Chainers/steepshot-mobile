namespace Sweetshot.Library.Models.Responses
{
    public class Comment
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
        public string MaxAcceptedPayout { get; set; }
        public string TotalPayoutReward { get; set; }
        public string Vote { get; set; }
    }

    public class GetCommentResponse
    {
        public Comment[] Comments { get; set; }
    }
}