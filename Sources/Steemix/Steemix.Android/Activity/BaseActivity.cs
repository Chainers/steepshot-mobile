using Android.OS;
using Android.Support.V7.App;
using Steemix.Library.HttpClient;

namespace Steemix.Android.Activity
{
    public class BaseActivity : AppCompatActivity
    {
        protected readonly SteemixApiClient ApiClient = new SteemixApiClient();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
    }
}