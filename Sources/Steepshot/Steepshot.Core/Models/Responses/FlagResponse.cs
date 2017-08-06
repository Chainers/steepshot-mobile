namespace Steepshot.Core.Models.Responses
{
    public class FlagResponse : MessageField
    {
        public Money NewTotalPayoutReward { get; set; }
        public bool IsFlagged => Message.Equals("Flagged");
    }
}