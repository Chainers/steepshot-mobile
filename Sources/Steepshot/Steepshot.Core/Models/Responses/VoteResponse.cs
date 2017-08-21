using Ditch;

namespace Steepshot.Core.Models.Responses
{
    public class VoteResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "Upvoted";
        private readonly bool _isVoted;

        public Money NewTotalPayoutReward { get; set; }
        
        public bool IsVoted => _isVoted || Message.Equals(ServerPositiveResponceMsg);

        public VoteResponse(bool isVoted)
        {
            _isVoted = isVoted;
            Message = ServerPositiveResponceMsg;
        }
    }
}