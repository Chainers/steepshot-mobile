using Android.App;
using Android.OS;
using Steepshot.Base;
using Steepshot.Fragment;
using Steepshot.View;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GuestActivity : BaseActivity, IFeedView
    {
		protected override void CreatePresenter() { }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
			var fragmentTransaction = SupportFragmentManager.BeginTransaction();
			CurrentHostFragment = HostFragment.NewInstance(new FeedFragment());
			fragmentTransaction.Add(Android.Resource.Id.Content, CurrentHostFragment);
	       	fragmentTransaction.Commit();
        }
	}
}
