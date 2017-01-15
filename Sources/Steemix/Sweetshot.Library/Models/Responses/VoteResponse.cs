namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "new_total_payout_reward": "0.00",
    ///  "status": "OK"
    ///}
    public class VoteResponse
    {
        public double NewTotalPayoutReward { get; set; }
        public string Status { get; set; }
    }
}