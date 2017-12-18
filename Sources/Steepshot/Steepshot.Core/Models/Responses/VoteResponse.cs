using System;
using Ditch.Core;

namespace Steepshot.Core.Models.Responses
{
    public class VoteResponse : VoidResponse
    {
        public Asset NewTotalPayoutReward { get; set; }

        public int NetVotes { get; set; }

        public DateTime VoteTime { get; set; }


        public VoteResponse(bool isSuccess) : base(isSuccess) { }
    }
}
