using Sweetshot.Library.Models.Responses.Common;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "message": "Upvoted",
    ///  "new_total_payout_reward": "0.00"
    ///}
    public class VoteResponse : MessageField
    {
        public double NewTotalPayoutReward { get; set; }
    }
}