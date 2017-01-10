using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Steemix.Library.Models.Requests;
using Steemix.Library.Models.Responses;

namespace Steemix.Droid
{
	public class FeedViewModel : MvvmViewModelBase
	{
		public ObservableCollection<UserPost> Posts = new ObservableCollection<UserPost>();

		public override void ViewLoad()
		{
			base.ViewLoad();
			if(Posts.Count==0)
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
			var _posts = await Manager.GetTopPosts(offset, limit);
			if (_posts != null)
			{
				foreach (var item in _posts)
				{
					Posts.Add(item);
				}
			}
		}

		public async Task<VoteResponse> Vote(UserPost post)
		{
			if (UserPrincipal.IsAuthenticated)
				return null;

			var voteRequest = new VoteRequest(UserPrincipal.CurrentUser.Token, post.Url);
			return await Manager.Vote(voteRequest, post.Vote);
		}
	}
}

