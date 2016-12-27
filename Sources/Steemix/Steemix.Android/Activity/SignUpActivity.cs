using Android.App;
using Android.OS;

namespace Steemix.Android.Activity
{
    [Activity(Label = "SteepShot")]
    public class SignUpActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_up);
        }
    }
}