using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace Steepshot
{
    [Activity(NoHistory = true, ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
	public class SignInActivity : BaseActivity, SignInView
    {
		SignInPresenter presenter;

        private EditText _username;
        private EditText _password;

		[InjectView(Resource.Id.loading_spinner)]
		ProgressBar spinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);
            
            _username = FindViewById<EditText>(Resource.Id.input_username);
            _password = FindViewById<EditText>(Resource.Id.input_password);

            if (UserPrincipal.Instance.CurrentUser != null)
            {
                _username.Text = UserPrincipal.Instance.CurrentUser.Login;
                _password.Text = UserPrincipal.Instance.CurrentUser.Password;
            }

            // TODO:KOA: Stub Login/Pass for test
            if (string.IsNullOrEmpty(_username.Text))
            {
#if DEBUG
                _username.Text = "joseph.kalu";
                _password.Text = "test12345";
#endif
            }

            _username.TextChanged += TextChanged;
            _password.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, global::Android.Text.TextChangedEventArgs e)
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

        [InjectOnClick(Resource.Id.sign_in_btn)]
        private async void SignInBtn_Click(object sender, System.EventArgs e)
        {
            var login = _username.Text.ToLower();
            var pass = _password.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                Toast.MakeText(this, "Invalid credentials", ToastLength.Short).Show();
                return;
            }

			spinner.Visibility = ViewStates.Visible;
			((AppCompatButton)sender).Visibility = ViewStates.Invisible;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
                return;

			var response = await presenter.SignIn(login, pass);

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
					spinner.Visibility = ViewStates.Invisible;
					((AppCompatButton)sender).Visibility = ViewStates.Visible;
                }
            }
            else
            {
                ShowAlert(Resource.String.error_connect_to_server);
				spinner.Visibility = ViewStates.Invisible;
				((AppCompatButton)sender).Visibility = ViewStates.Visible;
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

		protected override void CreatePresenter()
		{
			presenter = new SignInPresenter(this);
		}
	}
}