using System.Collections.Generic;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class BaseFeedPresenter : BasePresenter
    {
        public List<Post> Posts = new List<Post>();

		public async Task<OperationResult<VoteResponse>> Vote(int position)
		{
			var post = Posts[position];

			var voteRequest = new VoteRequest(User.UserInfo, !post.Vote, post.Url);
			var response = await Api.Vote(voteRequest);
			if (response.Success)
			{
				post.Vote = !Posts[position].Vote;
                if (post.NetVotes == -1)
                    post.NetVotes = 1;
                else
                {
					post.NetVotes = (post.Vote) ?
						post.NetVotes + 1 :
						post.NetVotes - 1;
                }
				post.TotalPayoutReward = response.Result.NewTotalPayoutReward;
                post.Flag = false;
			}
			return response;
		}

		public async Task<OperationResult<FlagResponse>> FlagPhoto(int position)
		{
			var post = Posts[position];
			var flagRequest = new FlagRequest(User.UserInfo, post.Flag, post.Url);
			var flagResponse = await Api.Flag(flagRequest);
			if (flagResponse.Success)
			{
				post.Flag = flagResponse.Result.IsFlagged;
				if (flagResponse.Result.IsFlagged)
				{
					if (post.Vote)
						if (post.NetVotes == 1)
							post.NetVotes = -1;
						else
							post.NetVotes--;
					post.Vote = false;
				}
			}
			return flagResponse;
		}
    }
}
