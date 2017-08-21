using Ditch;

namespace Steepshot.Core.Models.Responses
{
    public class FlagResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "Flagged";
        private readonly bool _isFlagged;
        
        public Money NewTotalPayoutReward { get; set; }

        public bool IsFlagged => _isFlagged || Message.Equals(ServerPositiveResponceMsg);

        public FlagResponse(bool isFlagged)
        {
            _isFlagged = isFlagged;
            Message = ServerPositiveResponceMsg;
        }
    }
}