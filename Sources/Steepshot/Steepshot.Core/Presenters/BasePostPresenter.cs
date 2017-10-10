using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
namespace Steepshot.Core.Presenters
{
    public class BasePostPresenter : ListPresenter
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

        protected BasePostPresenter()
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

        public async Task<List<string>> TryVote(Func<Post, bool> func)
        {
            Post post;
            lock (Posts)
                post = Posts.FirstOrDefault(func);

            if (post == null)
                return null;

            return await TryRunTask(Vote, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), post);
        }

        public async Task<List<string>> TryVote(int position)
        {
            Post post;
            lock (Posts)
                post = Posts[position];
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
            lock (Posts)
                post = Posts[position];
            if (post == null)
                return null;
            return await TryRunTask(Flag, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), post);
        }

        private async Task<List<string>> Flag(CancellationTokenSource cts, Post post)
        {
            var request = new VoteRequest(User.UserInfo, post.Flag ? VoteType.Flag : VoteType.Down, post.Url);
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