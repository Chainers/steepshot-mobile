using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class BaseFeedPresenter : ListPresenter
    {
        protected readonly List<Post> Posts;

        public override int Count => Posts.Count;

        public Post this[int position]
        {
            get
            {
                lock (Posts)
                {
                    if (position > -1 && position < Posts.Count)
                        return Posts[position];
                }
                return null;
            }
        }

        protected BaseFeedPresenter()
        {
            Posts = new List<Post>();
        }

        public void ClearPosts()
        {
            lock (Posts)
                Posts.Clear();
            IsLastReaded = false;
            OffsetUrl = string.Empty;
        }

        public void RemovePostsAt(int index)
        {
            lock (Posts)
                Posts.RemoveAt(index);
        }

        public int IndexOf(Func<Post, bool> func)
        {
            lock (Posts)
            {
                for (var i = 0; i < Posts.Count; i++)
                    if (func(Posts[i]))
                        return i;
            }
            return -1;
        }
        
        public async Task<List<string>> Vote(int position)
        {
            List<string> errors = null;
            try
            {
                Post post;
                lock (Posts)
                    post = Posts[position];
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
            catch (OperationCanceledException)
            {
                // to do nothing
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
                Post post;
                lock (Posts)
                    post = Posts[position];
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
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return errors;
        }
    }
}