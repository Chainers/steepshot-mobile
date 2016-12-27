using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Steemix.Library.HttpClient;

namespace Steemix.Android.Activity 
{
    public class BaseActivity : AppCompatActivity, IBaseModel
    {
        protected readonly SteemixApiClient ApiClient = new SteemixApiClient();

		public Context GetContext()
		{
			return this;
		}

		protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
    }
}