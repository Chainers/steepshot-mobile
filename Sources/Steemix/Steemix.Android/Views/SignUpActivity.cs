using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using Steemix.Library.Exceptions;
using Steemix.Library.Models.Requests;

namespace Steemix.Android.Activity
{
    [Activity]
	public class SignUpActivity : BaseActivity<SignUpViewModel>
    {
        private AppCompatButton SignUpBtn;
        private AppCompatButton SignInBtn;
		EditText username, postingkey, password; 

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_up);

            SignUpBtn = FindViewById<AppCompatButton>(Resource.Id.sign_up_btn);
            SignInBtn = FindViewById<AppCompatButton>(Resource.Id.sign_in_btn);

            SignUpBtn.Click += SignUpBtn_Click;
            SignInBtn.Click += SignInBtn_Click;

			username = FindViewById<EditText>(Resource.Id.input_username);
			postingkey = FindViewById<EditText>(Resource.Id.input_key);
			password = FindViewById<EditText>(Resource.Id.input_password);
		}

        private void SignInBtn_Click(object sender, System.EventArgs e)
        {
            var intent = new Intent(this, typeof(SignInActivity));
            StartActivity(intent);
        }
        
        private async void SignUpBtn_Click(object sender, System.EventArgs e)
        {
			var result = await ViewModel.SignUp(username.Text, password.Text, postingkey.Text);
			if (result)
			{
				ShowAlert(Resource.String.text_login);
			}
			else
			{
				ShowAlert(Resource.String.error_connect_to_server);
			}
        }


    }
}