using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class BasePostPresenter : ListPresenter<Post>
    {
        public void RemovePostsAt(int index)
        {
            lock (Items)
                Items.RemoveAt(index);
        }

        public int IndexOf(Func<Post, bool> func)
        {
            lock (Items)
            {
                for (var i = 0; i < Items.Count; i++)
                    if (func(Items[i]))
                        return i;
            }
            return -1;
        }

        public async Task<List<string>> TryVote(Func<Post, bool> func)
        {
            Post post;
            lock (Items)
                post = Items.FirstOrDefault(func);

            if (post == null)
                return null;

            return await TryRunTask(Vote, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), post);
        }

        public async Task<List<string>> TryVote(int position)
        {
            Post post;
            lock (Items)
                post = Items[position];
            if (post == null)
                return null;
            return await TryRunTask(Vote, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), post);
        }

        private async Task<List<string>> Vote(CancellationTokenSource cts, Post post)
        {
            var request = new VoteRequest(User.UserInfo, post.Vote ? VoteType.Down : VoteType.Up, post.Url);
            var response = await Api.Vote(request, cts);

            if (response.Success)
            {
                post.Vote = !post.Vote;
                post.Flag = false;
                post.TotalPayoutReward = response.Result.NewTotalPayoutReward;
                post.NetVotes = response.Result.NetVotes;
            }

            return response.Errors;
        }

        public async Task<List<string>> TryFlag(int position)
        {
            Post post;
            lock (Items)
                post = Items[position];
            if (post == null)
                return null;
            return await TryRunTask(Flag, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), post);
        }

        private async Task<List<string>> Flag(CancellationTokenSource cts, Post post)
        {
            var request = new VoteRequest(User.UserInfo, post.Flag ? VoteType.Down : VoteType.Flag, post.Url);
            var response = await Api.Vote(request);

            if (response.Success)
            {
                post.Flag = !post.Flag;
                post.Vote = false;
                post.TotalPayoutReward = response.Result.NewTotalPayoutReward;
                post.NetVotes = response.Result.NetVotes;
            }
            return response.Errors;
        }
    }
}
