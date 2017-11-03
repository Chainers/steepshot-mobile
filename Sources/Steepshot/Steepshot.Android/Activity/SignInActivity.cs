using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
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
using Steepshot.Utils;
using ZXing.Mobile;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class SignInActivity : BaseActivityWithPresenter<SignInPresenter>
    {
        public const string LoginExtraPath = "login";
        public const string AvatarUrlExtraPath = "avatar_url";

        private MobileBarcodeScanner _scanner;
        private string _username;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.profile_image)] private CircleImageView _profileImage;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.input_password)] private EditText _password;
        [InjectView(Resource.Id.qr_button)] private Button _buttonScanDefaultView;
        [InjectView(Resource.Id.sign_in_btn)] private AppCompatButton _signInBtn;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);

            MobileBarcodeScanner.Initialize(Application);
            _scanner = new MobileBarcodeScanner();
            _username = Intent.GetStringExtra(LoginExtraPath);
            var profileImage = Intent.GetStringExtra(AvatarUrlExtraPath);

            _backButton.Visibility = ViewStates.Visible;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Localization.Texts.PasswordViewTitleText;
            _signInBtn.Text = Localization.Texts.EnterAccountText;

            _viewTitle.Typeface = Style.Semibold;
            _password.Typeface = Style.Semibold;
            _signInBtn.Typeface = Style.Semibold;
            _buttonScanDefaultView.Typeface = Style.Semibold;
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
            if (!string.IsNullOrEmpty(profileImage))
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

                if (IsFinishing || IsDestroyed)
                    return;

                if (result != null)
                {
                    _password.Text = result.Text;
                    SignInBtn_Click(_signInBtn, null);
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                this.ShowAlert(Localization.Errors.Unknownerror, ToastLength.Short);
            }
        }

        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            var appCompatButton = (AppCompatButton)sender;

            var login = _username;
            var pass = _password?.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                this.ShowAlert(Localization.Errors.EmptyLogin, ToastLength.Short);
                return;
            }

            _spinner.Visibility = ViewStates.Visible;
            appCompatButton.Text = string.Empty;
            appCompatButton.Enabled = false;

            var response = await Presenter.TrySignIn(login, pass);
            if (IsFinishing || IsDestroyed)
                return;

            if (response != null && response.Success)
            {
                BasePresenter.User.AddAndSwitchUser(response.Result.SessionId, login, pass, BasePresenter.Chain, true);
                var intent = new Intent(this, typeof(RootActivity));
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(intent);
            }
            else
            {
                this.ShowAlert(response);
            }

            appCompatButton.Enabled = true;
            appCompatButton.Text = Localization.Texts.EnterAccountText;
            _spinner.Visibility = ViewStates.Invisible;
        }
    }
}
