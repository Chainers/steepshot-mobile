namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "comments": [
    ///    {
    ///      "body": "abcd",
    ///      "title": "abcd123",
    ///      "url": "/spam/@asduj/new-application-coming---#@joseph.kalu/re-new-application-coming----20161211t142239",
    ///      "category": "spam",
    ///      "author": "joseph.kalu",
    ///      "avatar": "",
    ///      "author_rewards": 0,
    ///      "author_reputation": 33,
    ///      "net_votes": 0,
    ///      "children": 0,
    ///      "created": "2016-12-11T14:22:39Z",
    ///      "curator_payout_value": 0.0,
    ///      "total_payout_value": 0.0,
    ///      "pending_payout_value": 0.0,
    ///      "max_accepted_payout": 1000000.0,
    ///      "total_payout_reward": 0.0,
    ///      "vote": 0
    ///    }
    ///  ]
    ///}
    public class GetCommentResponse
    {
        public Comment[] Comments { get; set; }
    }

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
}