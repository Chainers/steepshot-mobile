using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class BaseFeedPresenter : BasePresenter
    {
        private List<Post> _posts;

        public List<Post> Posts
        {
            get { return _posts; }
            set { _posts = value; }
        }

        protected BaseFeedPresenter()
        {
            _posts = new List<Post>();
        }
        
        public async Task<List<string>> Vote(int position)
        {
            List<string> errors = null;
            try
            {
                var post = Posts[position];
                var request = new VoteRequest(User.UserInfo, post.Vote ? VoteType.Down : VoteType.Up, post.Url);
                var response = await Api.Vote(request);
                errors = response.Errors;
                if (response.Success)
                {
                    post.Vote = !post.Vote;
                    post.Flag = false;
                    post.TotalPayoutReward = response.Result.NewTotalPayoutReward;
                    post.NetVotes = response.Result.NetVotes;
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return errors;
        }

        public async Task<List<string>> FlagPhoto(int position)
        {
            List<string> errors = null;
            try
            {
                var post = Posts[position];
                var request = new VoteRequest(User.UserInfo, post.Flag ? VoteType.Flag : VoteType.Down, post.Url);
                var response = await Api.Vote(request);
                errors = response.Errors;
                if (response.Success)
                {
                    post.Flag = !post.Flag;
                    post.Vote = false;
                    post.TotalPayoutReward = response.Result.NewTotalPayoutReward;
                    post.NetVotes = response.Result.NetVotes;
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return errors;
        }
    }
}