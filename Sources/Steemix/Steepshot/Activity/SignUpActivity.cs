using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Android.Views;

namespace Steepshot
{
    [Activity(NoHistory = true, ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
	public class SignUpActivity : BaseActivity, SignUpView
    {
		SignUpPresenter presenter;
        private EditText _username;
        private EditText _postingkey;
        private EditText _password;

        [InjectView(Resource.Id.loading_spinner)]
        ProgressBar spinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_up);
            Cheeseknife.Inject(this);

            _username = FindViewById<EditText>(Resource.Id.input_username);
            _postingkey = FindViewById<EditText>(Resource.Id.input_key);
            _password = FindViewById<EditText>(Resource.Id.input_password);
            _username.TextChanged += TextChanged;
            _username.TextChanged += TextChanged;
            _postingkey.TextChanged += TextChanged;
        }


        [InjectOnClick(Resource.Id.sign_in_btn)]
        private void SignInBtn_Click(object sender, System.EventArgs e)
        {
            var intent = new Intent(this, typeof(SignInActivity));
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.sign_up_btn)]
        private async void SignUpBtn_Click(object sender, System.EventArgs e)
        {
            var login = _username.Text.ToLower();
            var pass = _password.Text;
            var postingKey = _postingkey.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(postingKey))
            {
                Toast.MakeText(this, "Invalid credentials", ToastLength.Short).Show();
                return;
            }

            spinner.Visibility = ViewStates.Visible;
            ((Button)sender).Visibility = ViewStates.Invisible;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(postingKey))
                return;

			var response = await presenter.SignUp(login, pass, postingKey);

            if (response != null)
            {
                if (response.Success)
                {
                    UserPrincipal.Instance.CreatePrincipal(response.Result, login, pass);
                    var intent = new Intent(this, typeof(RootActivity));
                    intent.AddFlags(ActivityFlags.ClearTask);
                    StartActivity(intent);
                }
                else
                {
                    ShowAlert(response.Errors[0]);
                }
            }
            else
            {
                ShowAlert(Resource.String.error_connect_to_server);
            }
            spinner.Visibility = ViewStates.Invisible;
            ((Button)sender).Visibility = ViewStates.Visible;
        }

        private void TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var typedsender = (EditText)sender;
            if (string.IsNullOrWhiteSpace(e.Text.ToString()))
            {
                var message = GetString(Resource.String.error_empty_field);
                var d = GetDrawable(Resource.Drawable.ic_error);
                d.SetBounds(0, 0, d.IntrinsicWidth, d.IntrinsicHeight);
                typedsender.SetError(message, d);
            }
        }

		protected override void CreatePresenter()
		{
			presenter = new SignUpPresenter(this);
		}
	}
}