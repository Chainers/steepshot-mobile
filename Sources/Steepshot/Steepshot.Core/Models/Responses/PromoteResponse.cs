using System;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Responses
{
    public class PromoteResponse
    {
        public TimeSpan ExpectedUpvoteTime { get; private set; }

        public UserFriend Bot { get; private set; }

        public PromoteResponse(UserFriend bot, TimeSpan amount)
        {
            Bot = bot;
            ExpectedUpvoteTime = amount;
        }
    }
}
