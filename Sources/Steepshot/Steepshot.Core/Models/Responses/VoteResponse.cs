using System;

namespace Steepshot.Core.Models.Responses
{
    public class VoteResponse : VoidResponse
    {
        public double NewTotalPayoutReward { get; set; }

        public int NetVotes { get; set; }

        public DateTime VoteTime { get; set; }


        public VoteResponse(bool isSuccess) : base(isSuccess) { }
    }
}
