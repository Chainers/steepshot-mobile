using System;
using System.IO;
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
    public sealed class SignInActivity : BaseActivityWithPresenter<SignInPresenter>
    {
        public const string ChainExtraPath = "newChain";
        public const string LoginExtraPath = "login";
        public const string AvatarUrlExtraPath = "avatar_url";

        private MobileBarcodeScanner _scanner;
        private KnownChains _newChain;
        private string _username;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.profile_image)] private CircleImageView _profileImage;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.title)] private TextView _title;
        [InjectView(Resource.Id.input_password)] private EditText _password;
        [InjectView(Resource.Id.qr_button)] private Button _buttonScanDefaultView;
        [InjectView(Resource.Id.sign_in_btn)] private AppCompatButton _signInBtn;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);
            MobileBarcodeScanner.Initialize(Application);
            _scanner = new MobileBarcodeScanner();
            _username = Intent.GetStringExtra(LoginExtraPath);
            _title.Text = $"{Localization.Messages.Hello} {_username}";
            var profileImage = Intent.GetStringExtra(AvatarUrlExtraPath);
            _newChain = (KnownChains)Intent.GetIntExtra(ChainExtraPath, (int)KnownChains.None);

#if DEBUG
            try
            {
                var stream = Assets.Open("DebugWif.txt");
                using (var sr = new StreamReader(stream))
                {
                    var wif = sr.ReadToEnd();
                    _password.Text = wif;
                }
                stream.Dispose();
            }
            catch
            {
                //todo nothing
            }
#endif

            _password.TextChanged += TextChanged;

            if (string.IsNullOrEmpty(profileImage))
                Picasso.With(this).Load(Resource.Drawable.ic_user_placeholder).Into(_profileImage);
            else
                Picasso.With(this).Load(profileImage).Into(_profileImage);

            _buttonScanDefaultView.Click += OnButtonScanDefaultViewOnClick;
            _signInBtn.Click += SignInBtn_Click;
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
                {
                    _password.Text = result.Text;
                    SignInBtn_Click(_signInBtn, null);
                }
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

        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            var appCompatButton = sender as AppCompatButton;
            if (appCompatButton == null)
                return;
            var login = _username;
            var pass = _password?.Text;

            try
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
                {
                    ShowAlert(Localization.Errors.EmptyLogin, ToastLength.Short);
                    return;
                }

                _spinner.Visibility = ViewStates.Visible;
                appCompatButton.Visibility = ViewStates.Invisible;
                appCompatButton.Enabled = false;

                var response = await _presenter.TrySignIn(login, pass);
                if (response == null) // cancelled
                    return;

                if (response.Success)
                {
                    _newChain = KnownChains.None;
                    BasePresenter.User.AddAndSwitchUser(response.Result.SessionId, login, pass, BasePresenter.Chain, true);
                    var intent = new Intent(this, typeof(RootActivity));
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(intent);
                }
                else
                {
                    ShowAlert(response);
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                appCompatButton.Enabled = true;
                appCompatButton.Visibility = ViewStates.Visible;
                _spinner.Visibility = ViewStates.Invisible;
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
