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

            var voteRequest = new VoteRequest(User.UserInfo, post.Vote ? VoteType.Down : VoteType.Up, post.Url);
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

        public async Task<OperationResult<VoteResponse>> FlagPhoto(int position)
        {
            var post = Posts[position];
            var flagRequest = new VoteRequest(User.UserInfo, post.Flag ? VoteType.Flag : VoteType.Down, post.Url);
            var flagResponse = await Api.Vote(flagRequest);
            if (flagResponse.Success)
            {
                post.Flag = flagResponse.Result.IsSucces;
                if (flagResponse.Result.IsSucces)
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
