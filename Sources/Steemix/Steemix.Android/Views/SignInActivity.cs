using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Steemix.Library.Models.Requests;
using Android.Widget;

namespace Steemix.Android.Activity
{
    [Activity]
	public class SignInActivity : BaseActivity<SignInViewModel>
    {
        private AppCompatButton SignInBtn;
        private AppCompatButton ForgotPassBtn;
        private AppCompatButton SignUpBtn;
        
        EditText username, password;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_in);

            SignInBtn = FindViewById<AppCompatButton>(Resource.Id.sign_in_btn);
            ForgotPassBtn = FindViewById<AppCompatButton>(Resource.Id.forgot_pass_btn);
            SignUpBtn = FindViewById<AppCompatButton>(Resource.Id.sign_up_btn);

            SignInBtn.Click += SignInBtn_Click;
            ForgotPassBtn.Click += ForgotPassBtn_Click;
            SignUpBtn.Click += SignUpBtn_Click;

            username = FindViewById<EditText>(Resource.Id.input_username);
            password = FindViewById<EditText>(Resource.Id.input_password);

           
            username.Text = UserPrincipal.CurrentUser.Login;
            password.Text = UserPrincipal.CurrentUser.Password;

			username.Text = "joseph.kalu";
			password.Text = "test1234";
        }

		private async void SignInBtn_Click(object sender, System.EventArgs e)
        {
			var status = await ViewModel.SignIn(username.Text, password.Text);
			if (status)
			{
			    Finish();
			}
			else
			{ 
				ShowAlert(Resource.String.error_connect_to_server);
			}
        }

        private void ForgotPassBtn_Click(object sender, System.EventArgs e)
        {

        }

        private void SignUpBtn_Click(object sender, System.EventArgs e)
        {
            var intent = new Intent(this, typeof(SignUpActivity));
            StartActivity(intent);
        }
        
        private bool IsValid(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.username) || string.IsNullOrEmpty(request.password))
                return false;
            return true;
        }
    }
}