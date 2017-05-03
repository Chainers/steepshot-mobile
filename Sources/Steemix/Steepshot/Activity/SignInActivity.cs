using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Square.Picasso;

namespace Steepshot
{
    [Activity(NoHistory = true, ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
	public class SignInActivity : BaseActivity, SignInView
    {
		SignInPresenter presenter;

		private string _username;
        private EditText _password;

		[InjectView(Resource.Id.profile_image)]
		CircleImageView ProfileImage;

		[InjectView(Resource.Id.loading_spinner)]
		ProgressBar spinner;

		[InjectView(Resource.Id.title)]
		TextView title;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);
            
			_username = Intent.GetStringExtra("login");
			title.Text = $"Hello, {_username}";
			var profileImage = Intent.GetStringExtra("avatar_url");
            
            _password = FindViewById<EditText>(Resource.Id.input_password);

            if (UserPrincipal.Instance.CurrentUser != null)
            {
                _password.Text = UserPrincipal.Instance.CurrentUser.Password;
            }

            // TODO:KOA: Stub Login/Pass for test
            
#if DEBUG
            _password.Text = "5JXCxj6YyyGUTJo9434ZrQ5gfxk59rE3yukN42WBA6t58yTPRTG";
#endif
            
            _password.TextChanged += TextChanged;

			if (!string.IsNullOrEmpty(profileImage))
                    Picasso.With(this).Load(profileImage).Into(ProfileImage);
                else
                    Picasso.With(this).Load(Resource.Drawable.ic_user_placeholder).Into(ProfileImage);
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
            var login = _username;
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
					UserPrincipal.Instance.CreatePrincipal(response.Result, login, pass, UserPrincipal.Instance.CurrentNetwork);
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

        [InjectOnClick(Resource.Id.sign_up_btn)]
        private void SignUpBtn_Click(object sender, System.EventArgs e)
        {
            /*var intent = new Intent(this, typeof(SignUpActivity));
            StartActivity(intent);*/
        }

		protected override void CreatePresenter()
		{
			presenter = new SignInPresenter(this);
		}
	}
}