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
using ZXing.Mobile;

namespace Steepshot
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
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

		MobileBarcodeScanner scanner;
		private string _newAccountNetwork;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);
            MobileBarcodeScanner.Initialize (Application);
			scanner = new MobileBarcodeScanner();
			_username = Intent.GetStringExtra("login");
			title.Text = $"Hello, {_username}";
			var profileImage = Intent.GetStringExtra("avatar_url");
            _newAccountNetwork = Intent.GetStringExtra("newNetwork");
            _password = FindViewById<EditText>(Resource.Id.input_password);

            // TODO:KOA: Stub Login/Pass for test
            
#if DEBUG
            _password.Text = "5JXCxj6YyyGUTJo9434ZrQ5gfxk59rE3yukN42WBA6t58yTPRTG";
#endif
            
            _password.TextChanged += TextChanged;

			if (!string.IsNullOrEmpty(profileImage))
                    Picasso.With(this).Load(profileImage).Into(ProfileImage);
                else
                    Picasso.With(this).Load(Resource.Drawable.ic_user_placeholder).Into(ProfileImage);
			
			var buttonScanDefaultView = this.FindViewById<Button>(Resource.Id.qr_button);
			buttonScanDefaultView.Click += async (object sender, EventArgs e) =>
			{
				try
				{
					//Tell our scanner to use the default overlay
					scanner.UseCustomOverlay = false;

					//We can customize the top and bottom text of the default overlay
					scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
					scanner.BottomText = "Wait for the barcode to automatically scan!";

					//Start scanning
					var result = await scanner.Scan();
					_password.Text = result.Text;
				}
				catch (Exception ex)
				{
					
				}
			};
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
					_newAccountNetwork = null;
					UserPrincipal.Instance.CreatePrincipal(response.Result, login, pass, UserPrincipal.Instance.CurrentNetwork);
                    var intent = new Intent(this, typeof(RootActivity));
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
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

		protected override void OnDestroy()
		{
			if (!string.IsNullOrEmpty(_newAccountNetwork))
			{
				UserPrincipal.Instance.CurrentNetwork = _newAccountNetwork == Constants.Steem ? Constants.Golos : Constants.Steem;
				BasePresenter.SwitchNetwork();
			}
			base.OnDestroy();
		}

		protected override void CreatePresenter()
		{
			presenter = new SignInPresenter(this);
		}
	}
}