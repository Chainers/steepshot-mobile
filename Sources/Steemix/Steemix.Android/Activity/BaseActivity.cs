using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Steemix.Library.HttpClient;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Steemix.Android.Activity 
{
    public class BaseActivity : AppCompatActivity, IBaseModel
    {
        protected readonly SteemixApiClient ApiClient = new SteemixApiClient();
        protected static string UserName;
        protected static string Token;

		public Context GetContext()
		{
			return this;
		}

		protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected virtual void ShowAlert(int messageid)
        {
            var message = GetString(messageid);
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Error");
            alert.SetMessage(message);
            alert.SetPositiveButton("Ok", ((senderAlert, args) => { }));
            Dialog dialog = alert.Create();
            dialog.Show();
        }
    }
}