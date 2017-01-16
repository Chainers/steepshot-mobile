using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "offset": "/spam/@joseph.kalu/test-post-thu-jan-12-101304-2017",
    ///  "count": 9,
    ///  "results": [
    ///    {
    ///      "body": "http://res.cloudinary.com/steepshot2/image/upload/v1484422967/zmfes9yxqi6naoram6ij.jpg",
    ///      "title": "Test post Sat Jan 14 19:42:45 2017",
    ///      "url": "/spam/@joseph.kalu/test-post-sat-jan-14-194245-2017",
    ///      "category": "spam",
    ///      "author": "joseph.kalu",
    ///      "avatar": "http://vignette2.wikia.nocookie.net/tomandjerry/images/6/6d/Tom-tom-and-jerry.png/revision/latest?cb=20140627113049",
    ///      "author_rewards": 0,
    ///      "author_reputation": 37,
    ///      "net_votes": 5,
    ///      "children": 0,
    ///      "created": "2017-01-14T19:42:48Z",
    ///      "curator_payout_value": 0.0,
    ///      "total_payout_value": 0.0,
    ///      "pending_payout_value": 0.0,
    ///      "max_accepted_payout": 1000000.0,
    ///      "total_payout_reward": 0.0,
    ///      "vote": false,
    ///      "tags": [
    ///        "test"
    ///      ],
    ///      "depth": 0
    ///    }
    ///  ]
    ///}
    public class UserPostResponse
    {
        public string Offset { get; set; }
        public int Count { get; set; }
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