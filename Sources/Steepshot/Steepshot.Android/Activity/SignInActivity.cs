using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
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
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using ZXing.Mobile;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class SignInActivity : BaseActivityWithPresenter<SignInPresenter>, ITarget
    {
        public const string LoginExtraPath = "login";
        public const string AvatarUrlExtraPath = "avatar_url";

        private MobileBarcodeScanner _scanner;
        private string _username;
        private string _profileImageUrl;

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
        [InjectView(Resource.Id.root_layout)] private RelativeLayout _rootLayout;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Inject(this);

            MobileBarcodeScanner.Initialize(Application);
            _scanner = new MobileBarcodeScanner();
            _username = Intent.GetStringExtra(LoginExtraPath);
            _profileImageUrl = Intent.GetStringExtra(AvatarUrlExtraPath);

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PasswordViewTitleText);
            _signInBtn.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SignIn);
            _buttonScanDefaultView.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ScanQRCode);

            _viewTitle.Typeface = Style.Semibold;
            _password.Typeface = Style.Semibold;
            _signInBtn.Typeface = Style.Semibold;
            _buttonScanDefaultView.Typeface = Style.Semibold;
#if DEBUG
            var di = AppSettings.AssetsesHelper.GetDebugInfo();
            _password.Text = BasePresenter.Chain == KnownChains.Golos
                ? di.GolosTestWif
                : di.SteemTestWif;
#endif
            if (!string.IsNullOrEmpty(_profileImageUrl))
                Picasso.With(this).Load(_profileImageUrl)
                       .Placeholder(Resource.Drawable.ic_holder)
                       .NoFade()
                       .Resize(300, 0)
                       .Priority(Picasso.Priority.Normal)
                       .Into(_profileImage, OnSuccess, OnError);

            _buttonScanDefaultView.Click += OnButtonScanDefaultViewOnClick;
            _signInBtn.Click += SignInBtn_Click;
            _rootLayout.Click += HideKeyboard;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        private async void OnButtonScanDefaultViewOnClick(object sender, EventArgs e)
        {
            if (PermissionChecker.CheckSelfPermission(this, Android.Manifest.Permission.Camera) == (int)Permission.Granted
                    && PermissionChecker.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                try
                {
                    //Tell our scanner to use the default overlay
                    _scanner.UseCustomOverlay = false;

                    //We can customize the top and bottom text of the default overlay
                    _scanner.TopText = AppSettings.LocalizationManager.GetText(LocalizationKeys.CameraHoldUp);
                    _scanner.BottomText = AppSettings.LocalizationManager.GetText(LocalizationKeys.WaitforScan);

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
                    this.ShowAlert(LocalizationKeys.UnknownError, ToastLength.Short);
                }
            }
            else
            {
                //Replace for Permission request
                this.ShowAlert(LocalizationKeys.CheckPermission);
            }
        }

        private void GoBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            var appCompatButton = (AppCompatButton)sender;

            var login = _username;
            var pass = _password?.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                this.ShowAlert(LocalizationKeys.EmptyLogin, ToastLength.Short);
                return;
            }

            _spinner.Visibility = ViewStates.Visible;
            appCompatButton.Text = string.Empty;
            appCompatButton.Enabled = false;

            var response = await Presenter.TrySignIn(login, pass);
            if (IsFinishing || IsDestroyed)
                return;

            if (response.IsSuccess)
            {
                BasePresenter.User.AddAndSwitchUser(login, pass, BasePresenter.Chain, true);
                var intent = new Intent(this, typeof(RootActivity));
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(intent);
            }
            else
            {
                this.ShowAlert(response.Error);
            }

            appCompatButton.Enabled = true;
            appCompatButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EnterAccountText);
            _spinner.Visibility = ViewStates.Invisible;
        }

        private void HideKeyboard(object sender, EventArgs e)
        {
            HideKeyboard();
        }

        private void OnSuccess()
        {
        }

        private void OnError()
        {
            Picasso.With(this).Load(_profileImageUrl).NoFade().Into(this);
        }

        public void OnBitmapFailed(Drawable p0)
        {
        }

        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            _profileImage.SetImageBitmap(p0);
        }

        public void OnPrepareLoad(Drawable p0)
        {
        }
    }
}
