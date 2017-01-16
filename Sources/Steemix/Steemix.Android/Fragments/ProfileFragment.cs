using System;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.Activities;

namespace Steemix.Droid.Fragments
{
    public class ProfileFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
            Cheeseknife.Inject(this, v);
            return v;
        }

        [InjectOnClick(Resource.Id.btn_settings)]
        public void OnSettingsClick(object sender, EventArgs e)
        {
            var intent = new Intent(Context, typeof(SettingsActivity));
            StartActivity(intent);
        }


        [InjectOnClick(Resource.Id.btn_following)]
        public void OnFollowingClick(object sender, EventArgs e)
        {
            var intent = new Intent(Context, typeof(FollowersActivity));
            intent.PutExtra("isFollow", false);
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.btn_followers)]
        public void OnFollowersClick(object sender, EventArgs e)
        {
            var intent = new Intent(Context, typeof(FollowersActivity));
            intent.PutExtra("isFollow", true);
            StartActivity(intent);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            Cheeseknife.Reset(this);
        }
    }
}
