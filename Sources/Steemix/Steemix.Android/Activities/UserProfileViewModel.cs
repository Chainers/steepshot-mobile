using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
	public class UserProfileViewModel : MvvmViewModelBase
	{

		private UserProfileResponse userData;

		private UserPostResponse postsData;

		public ObservableCollection<Post> UserPosts = new ObservableCollection<Post>();

		public async Task<UserProfileResponse> GetUserInfo(string user, bool requireUpdate = false)
		{
			if (requireUpdate || userData == null)
			{
				var req = new UserProfileRequest(user);
				var response = await ViewModelLocator.Api.GetUserProfile(req);
				userData = response.Result;
			}
			return userData;
		}

		public async Task<UserPostResponse> GetUserPosts()
		{
			var req = new UserPostsRequest(userData.Username);
			var response = await ViewModelLocator.Api.GetUserPosts(req);
			postsData = response.Result;

			foreach (var item in response.Result.Results)
			{
				UserPosts.Add(item);
			}

			return postsData;
		}

		public string GetPostsOffset()
		{
			if (postsData != null)
				return postsData.Offset;

			return null;
		}
	}
}
