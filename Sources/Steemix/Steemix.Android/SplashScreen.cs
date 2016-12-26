using Android.App;
using Android.OS;

namespace Steemix.Android
{
    [Activity(Label = "SteepShot", MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : global::Android.App.Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.lyt_sign_in);
        }
    }
}