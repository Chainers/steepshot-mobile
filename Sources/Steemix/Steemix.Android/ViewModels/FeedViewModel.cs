using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Steemix.Library.Models.Responses;

namespace Steemix.Android
{
	public class FeedViewModel : MvvmViewModelBase
	{
		public ObservableCollection<UserPost> Posts = new ObservableCollection<UserPost>();

		public FeedViewModel()
		{
		}

		public override void ViewLoad()
		{
			base.ViewLoad();
			GetTopPosts(string.Empty, 20);
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
	}
}

