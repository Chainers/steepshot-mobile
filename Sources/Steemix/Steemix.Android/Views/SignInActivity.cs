using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steemix.Droid.Activity;

namespace Steemix.Droid.Views
{
    [Activity(NoHistory = true)]
    public class SignInActivity : BaseActivity<SignInViewModel>
    {
        private AppCompatButton _signInBtn;
        private AppCompatButton _forgotPassBtn;
        private AppCompatButton _signUpBtn;
        private EditText _username;
        private EditText _password;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_in);

            _signInBtn = FindViewById<AppCompatButton>(Resource.Id.sign_in_btn);
            _forgotPassBtn = FindViewById<AppCompatButton>(Resource.Id.forgot_pass_btn);
            _signUpBtn = FindViewById<AppCompatButton>(Resource.Id.sign_up_btn);

            _forgotPassBtn.Visibility = ViewStates.Invisible; // TODO:KOA-COM: ñïðàÿòàíà ïî çàäà÷å SS-1: Login screen 

            _signInBtn.Click += SignInBtn_Click;
            _forgotPassBtn.Click += ForgotPassBtn_Click;
            _signUpBtn.Click += SignUpBtn_Click;

            _username = FindViewById<EditText>(Resource.Id.input_username);
            _password = FindViewById<EditText>(Resource.Id.input_password);

            _username.Text = UserPrincipal.CurrentUser.Login;
            _password.Text = UserPrincipal.CurrentUser.Password;

            // TODO:KOA: óäàëèòü ïîñëå òåñòèðîâàíèÿ
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

        private async void SignInBtn_Click(object sender, System.EventArgs e)
        {
            var login = _username.Text;
            var pass = _password.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
                return;

            var response = await ViewModel.SignIn(login, pass);

            if (response != null)
            {
                if (string.IsNullOrEmpty(response.error))
                {
                    UserPrincipal.CreatePrincipal(response, login, pass);
                    Finish();
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

        private void ForgotPassBtn_Click(object sender, System.EventArgs e)
        {

        }

        private void SignUpBtn_Click(object sender, System.EventArgs e)
        {
            var intent = new Intent(this, typeof(SignUpActivity));
            StartActivity(intent);
        }
    }
}