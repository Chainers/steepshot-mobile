using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
    public class FeedViewModel : MvvmViewModelBase
    {
        public ObservableCollection<UserPost> Posts = new ObservableCollection<UserPost>();

        public override void ViewLoad()
        {
            base.ViewLoad();
            if (Posts.Count == 0)
                Task.Run(() => GetTopPosts(string.Empty, 20));
        }

        public override void ViewAppear()
        {
            base.ViewAppear();
        }

        public override void ViewDisappear()
        {
            base.ViewDisappear();
        }

        public async Task GetTopPosts(string offset, int limit)
        {
            var postrequest = new PostsRequest(PostType.Top, limit, offset);
            var posts = await ViewModelLocator.Api.GetPosts(postrequest);
            //TODO:KOA -- Errors not processed
            if (posts.Success)
            {
                foreach (var item in posts.Result.Results)
                {
                    Posts.Add(item);
                }
            }
        }

        public async Task<OperationResult<VoteResponse>> Vote(UserPost post)
        {
            if (!UserPrincipal.IsAuthenticated)
                return new OperationResult<VoteResponse> { Errors = new List<string> { "Forbidden" }, Success = false };

            var voteRequest = new VoteRequest(UserPrincipal.CurrentUser.SessionId, post.Vote, post.Url);
            return await ViewModelLocator.Api.Vote(voteRequest);
        }
    }
}