using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using ZXing.Mobile;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SignInActivity : BaseActivityWithPresenter<SignInPresenter>
    {
        private string _username;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.profile_image)] CircleImageView _profileImage;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar _spinner;
        [InjectView(Resource.Id.title)] TextView _title;
        [InjectView(Resource.Id.input_password)] EditText _password;
        [InjectView(Resource.Id.qr_button)] Button _buttonScanDefaultView;
#pragma warning restore 0649

        MobileBarcodeScanner _scanner;
        private KnownChains _newChain;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);
            MobileBarcodeScanner.Initialize(Application);
            _scanner = new MobileBarcodeScanner();
            _username = Intent.GetStringExtra("login");
            _title.Text = $"Hello, {_username}";
            var profileImage = Intent.GetStringExtra("avatar_url");
            _newChain = (KnownChains)Intent.GetIntExtra("newChain", (int)KnownChains.None);

#if DEBUG
            _password.Text = "***REMOVED***";
#endif

            _password.TextChanged += TextChanged;

            if (!string.IsNullOrEmpty(profileImage))
                Picasso.With(this).Load(profileImage).Into(_profileImage);
            else
                Picasso.With(this).Load(Resource.Drawable.ic_user_placeholder).Into(_profileImage);

            _buttonScanDefaultView.Click += OnButtonScanDefaultViewOnClick;
        }

        private async void OnButtonScanDefaultViewOnClick(object sender, EventArgs e)
        {
            try
            {
                //Tell our scanner to use the default overlay
                _scanner.UseCustomOverlay = false;

                //We can customize the top and bottom text of the default overlay
                _scanner.TopText = Localization.Messages.CameraHoldUp;
                _scanner.BottomText = Localization.Messages.WaitforScan;

                //Start scanning
                var result = await _scanner.Scan();
                if (result != null)
                    _password.Text = result.Text;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        private void TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var typedsender = (EditText)sender;
            if (string.IsNullOrWhiteSpace(e?.Text.ToString()))
            {
                var message = GetString(Resource.String.error_empty_field);
                var d = ContextCompat.GetDrawable(this, Resource.Drawable.ic_error);
                d.SetBounds(0, 0, d.IntrinsicWidth, d.IntrinsicHeight);
                typedsender.SetError(message, d);
            }
        }

        [InjectOnClick(Resource.Id.sign_in_btn)]
        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var login = _username;
                var pass = _password.Text;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
                {
                    Toast.MakeText(this, Localization.Errors.EmptyLogin, ToastLength.Short).Show();
                    return;
                }

                _spinner.Visibility = ViewStates.Visible;
                ((AppCompatButton)sender).Visibility = ViewStates.Invisible;
                ((AppCompatButton)sender).Enabled = false;

                var response = await _presenter.SignIn(login, pass);

                if (response != null)
                {
                    if (response.Success)
                    {
                        _newChain = KnownChains.None;
                        BasePresenter.User.AddAndSwitchUser(response.Result.SessionId, login, pass, BasePresenter.Chain);
                        var intent = new Intent(this, typeof(RootActivity));
                        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        StartActivity(intent);
                    }
                    else
                    {
                        ShowAlert(response.Errors[0]);
                        _spinner.Visibility = ViewStates.Invisible;
                        ((AppCompatButton)sender).Visibility = ViewStates.Visible;
                        ((AppCompatButton)sender).Enabled = true;
                    }
                }
                else
                {
                    ShowAlert(Resource.String.error_connect_to_server);
                    _spinner.Visibility = ViewStates.Invisible;
                    ((AppCompatButton)sender).Visibility = ViewStates.Visible;
                    ((AppCompatButton)sender).Enabled = true;
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        protected override void OnDestroy()
        {
            if (_newChain != KnownChains.None)
            {
                BasePresenter.SwitchChain(_newChain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem);
            }
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        protected override void CreatePresenter()
        {
            _presenter = new SignInPresenter();
        }
    }
}