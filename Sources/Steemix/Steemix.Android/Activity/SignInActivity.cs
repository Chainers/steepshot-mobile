using Android.App;
using Android.OS;

namespace Steemix.Android.Activity
{
    [Activity(Label = "SteepShot", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class SignInActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_in);
        }
    }
}