using System.Collections.Generic;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class CommentsPresenter : BasePresenter
    {
        public List<Post> Posts;

        public async Task<List<Post>> GetComments(string postUrl)
        {
            var request = new NamedInfoRequest(postUrl)
            {
                Login = User.Login
            };

            var result = await Api.GetComments(request);
            Posts = result.Result.Results;
            return Posts;
        }

        public async Task<OperationResult<VoteResponse>> Vote(Post post)
        {
            if (!User.IsAuthenticated)
                return new OperationResult<VoteResponse> { Errors = new List<string> { "Forbidden" } };

            int diezid = post.Url.IndexOf('#');
            string posturl = post.Url.Substring(diezid + 1);

            var voteRequest = new VoteRequest(User.UserInfo, post.Vote ? VoteType.Down : VoteType.Up, posturl);
            return await Api.Vote(voteRequest);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(string comment, string url)
        {
            var reqv = new CreateCommentRequest(User.UserInfo, url, comment, comment);
            return await Api.CreateComment(reqv);
        }
    }
}