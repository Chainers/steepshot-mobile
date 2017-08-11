using Ditch;

namespace Steepshot.Core.Models.Responses
{
    /// {
    ///   "message": "Upvoted",
    ///   "new_total_payout_reward": "0.00"
    /// }
    public class VoteResponse : MessageField
    {
        public Money NewTotalPayoutReward { get; set; }
        public bool IsVoted => Message.Equals("Upvoted");
    }
}