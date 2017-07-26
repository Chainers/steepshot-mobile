using Android.App;
using Android.OS;

namespace Steepshot
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GuestActivity : BaseActivity, FeedView
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
