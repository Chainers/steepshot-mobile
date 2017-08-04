namespace Steepshot.Core.Models.Responses
{
    public class FlagResponse : MessageField
    {
        public double NewTotalPayoutReward { get; set; }
        public bool IsFlagged => Message.Equals("Flagged");
    }
}