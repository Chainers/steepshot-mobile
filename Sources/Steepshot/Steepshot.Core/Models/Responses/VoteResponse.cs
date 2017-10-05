using Ditch;

namespace Steepshot.Core.Models.Responses
{
    public class VoteResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "Upvoted";
        private const string ServerPositiveResponceMsg2 = "Flagged";

        private readonly bool _isSucces;

        public Money NewTotalPayoutReward { get; set; }

        public bool IsSucces => _isSucces || Message.Equals(ServerPositiveResponceMsg) || Message.Equals(ServerPositiveResponceMsg2);

        public int NetVotes { get; internal set; }

        public VoteResponse(bool isSucces)
        {
            _isSucces = isSucces;
        }
    }
}