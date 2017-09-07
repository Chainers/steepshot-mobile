using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class BaseFeedPresenter : BasePresenter
    {
        public List<Post> Posts = new List<Post>();

        public async Task<List<string>> Vote(int position)
        {
            List<string> errors = null;
            try
            {
                var post = Posts[position];
                var voteRequest = new VoteRequest(User.UserInfo, post.Vote ? VoteType.Down : VoteType.Up, post.Url);
                var response = await Api.Vote(voteRequest);
                errors = response.Errors;
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
            }
            catch(Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            return errors;
        }

        public async Task<List<string>> FlagPhoto(int position)
        {
            List<string> errors = null;
            try
            {
                var post = Posts[position];
                var flagRequest = new VoteRequest(User.UserInfo, post.Flag ? VoteType.Flag : VoteType.Down, post.Url);
                var flagResponse = await Api.Vote(flagRequest);
                errors = flagResponse.Errors;
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
            }
            catch(Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            return errors;
        }
    }
}
