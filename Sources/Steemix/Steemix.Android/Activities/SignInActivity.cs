using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.ViewModels;

namespace Steemix.Droid.Activities
{
    [Activity(NoHistory = true)]
    public class SignInActivity : BaseActivity<SignInViewModel>
    {
        private EditText _username;
        private EditText _password;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);

            // TODO:KOA-COM: NotReadyYet
            var forgotPassBtn = FindViewById<AppCompatButton>(Resource.Id.forgot_pass_btn);
            forgotPassBtn.Visibility = ViewStates.Invisible; 
       

            _username = FindViewById<EditText>(Resource.Id.input_username);
            _password = FindViewById<EditText>(Resource.Id.input_password);

            _username.Text = UserPrincipal.CurrentUser.Login;
            _password.Text = UserPrincipal.CurrentUser.Password;

            // TODO:KOA: Stub Login/Pass for test
            if (string.IsNullOrEmpty(_username.Text))
            {
                _username.Text = "joseph.kalu";
                _password.Text = "test1234";
            }

            _username.TextChanged += TextChanged;
            _username.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, global::Android.Text.TextChangedEventArgs e)
        {
            var typedsender = (EditText)sender;
            if (string.IsNullOrWhiteSpace(e.Text.ToString()))
            {
                typedsender.SetBackgroundColor(Color.Red);
                var message = GetString(Resource.String.error_empty_field);
                typedsender.SetError(message, null);
            }
            else
            {
                typedsender.SetBackgroundColor(Color.White);
                typedsender.SetError(string.Empty, null);
            }
        }

        [InjectOnClick(Resource.Id.sign_in_btn)]
        private async void SignInBtn_Click(object sender, System.EventArgs e)
        {
            var login = _username.Text;
            var pass = _password.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
                return;

            var response = await ViewModel.SignIn(login, pass);

            if (response != null)
            {
                if (response.Success)
                {
                    UserPrincipal.CreatePrincipal(response.Result, login, pass);
					var intent = new Intent(this, typeof(RootActivity));
					intent.AddFlags(ActivityFlags.ClearTask);
                    StartActivity(intent);
                }
                else
                {
                    ShowAlert(Resource.String.error_connect_to_server);
                }
            }
            else
            {
                ShowAlert(Resource.String.error_connect_to_server);
            }
        }

        [InjectOnClick(Resource.Id.forgot_pass_btn)]
        private void ForgotPassBtn_Click(object sender, System.EventArgs e)
        {

        }

        [InjectOnClick(Resource.Id.sign_up_btn)]
        private void SignUpBtn_Click(object sender, System.EventArgs e)
        {
            var intent = new Intent(this, typeof(SignUpActivity));
            StartActivity(intent);
        }
    }
}