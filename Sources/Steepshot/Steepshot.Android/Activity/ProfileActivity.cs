using Android.App;
using Android.OS;
using Steepshot.Base;
using Steepshot.Fragment;
using Steepshot.View;

namespace Steepshot.Activity
{
	[Activity(Label = "ProfileActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class ProfileActivity : BaseActivity, UserProfileView
	{
		protected override void CreatePresenter() { }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			var fragmentTransaction = SupportFragmentManager.BeginTransaction();
			fragmentTransaction.Add(Android.Resource.Id.Content, new ProfileFragment(Intent.GetStringExtra("ID")));
			fragmentTransaction.Commit();
		}
	}
}
