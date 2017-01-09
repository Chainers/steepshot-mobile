using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using Steemix.Android.Activity;

namespace Steemix.Android.Views
{
    [Activity(NoHistory = true)]
    public class SignUpActivity : BaseActivity<SignUpViewModel>
    {
        private AppCompatButton _signUpBtn;
        private AppCompatButton _signInBtn;
        private EditText _username;
        private EditText _postingkey;
        private EditText _password;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_sign_up);

            _signUpBtn = FindViewById<AppCompatButton>(Resource.Id.sign_up_btn);
            _signInBtn = FindViewById<AppCompatButton>(Resource.Id.sign_in_btn);

            _signUpBtn.Click += SignUpBtn_Click;
            _signInBtn.Click += SignInBtn_Click;

            _username = FindViewById<EditText>(Resource.Id.input_username);
            _postingkey = FindViewById<EditText>(Resource.Id.input_key);
            _password = FindViewById<EditText>(Resource.Id.input_password);


            _username.TextChanged += TextChanged;
            _username.TextChanged += TextChanged;
            _postingkey.TextChanged += TextChanged;
        }

        private void SignInBtn_Click(object sender, System.EventArgs e)
        {
            var intent = new Intent(this, typeof(SignInActivity));
            StartActivity(intent);
        }

        private async void SignUpBtn_Click(object sender, System.EventArgs e)
        {
            var login = _username.Text;
            var pass = _password.Text;
            var postingKey = _postingkey.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(postingKey))
                return;

            var response = await ViewModel.SignUp(login, pass, postingKey);

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
    }
}