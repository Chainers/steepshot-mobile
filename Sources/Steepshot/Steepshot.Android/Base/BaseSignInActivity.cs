using System;
using System.Linq;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using ZXing.Mobile;

namespace Steepshot.Base
{
    public abstract class BaseSignInActivity : BaseActivity
    {
        private MobileBarcodeScanner _scanner;
        protected string Username;
        protected string ProfileImageUrl;
        protected AccountInfoResponse AccountInfoResponse;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.profile_image)] protected CircleImageView _profileImage;
        [BindView(Resource.Id.loading_spinner)] protected ProgressBar _spinner;
        [BindView(Resource.Id.input_password)] protected EditText _password;
        [BindView(Resource.Id.qr_button)] protected Button _buttonScanDefaultView;
        [BindView(Resource.Id.sign_in_btn)] protected AppCompatButton _signInBtn;
        [BindView(Resource.Id.profile_login)] protected TextView _viewTitle;
        [BindView(Resource.Id.btn_switcher)] protected ImageButton _switcher;
        [BindView(Resource.Id.btn_settings)] protected ImageButton _settings;
        [BindView(Resource.Id.btn_back)] protected ImageButton _backButton;
        [BindView(Resource.Id.root_layout)] protected RelativeLayout _rootLayout;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_sign_in);
            Cheeseknife.Bind(this);

            MobileBarcodeScanner.Initialize(Application);
            _scanner = new MobileBarcodeScanner();

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBack;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = App.Localization.GetText(LocalizationKeys.PasswordViewTitleText);
            _signInBtn.Text = App.Localization.GetText(LocalizationKeys.SignIn);
            _buttonScanDefaultView.Text = App.Localization.GetText(LocalizationKeys.ScanQRCode);

            _viewTitle.Typeface = Style.Semibold;
            _password.Typeface = Style.Semibold;
            _signInBtn.Typeface = Style.Semibold;
            _buttonScanDefaultView.Typeface = Style.Semibold;
#if DEBUG
            var asset = App.Container.GetAssetHelper();
            var di = asset.GetDebugInfo();
            _password.Text = App.MainChain == KnownChains.Golos
                ? di.GolosTestWif
                : di.SteemTestWif;
#endif            
            if (!string.IsNullOrEmpty(ProfileImageUrl))
                Picasso.With(this).Load(ProfileImageUrl.GetImageProxy(_profileImage.LayoutParameters.Width, _profileImage.LayoutParameters.Height))
                       .Placeholder(Resource.Drawable.ic_holder)
                       .NoFade()
                       .Priority(Picasso.Priority.Normal)
                       .Into(_profileImage, null, () =>
                        Picasso.With(this).Load(ProfileImageUrl).NoFade().Into(_profileImage));

            _buttonScanDefaultView.Click += OnButtonScanDefaultViewOnClick;
            _signInBtn.Click += SignIn;
            _rootLayout.Click += HideKeyboard;
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == CommonPermissionsRequestCode && !grantResults.Any(x => x != Permission.Granted))
                ScanQr();
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void ScanQr()
        {
            try
            {
                //Tell our scanner to use the default overlay
                _scanner.UseCustomOverlay = false;

                //We can customize the top and bottom text of the default overlay
                _scanner.TopText = App.Localization.GetText(LocalizationKeys.CameraHoldUp);
                _scanner.BottomText = App.Localization.GetText(LocalizationKeys.WaitforScan);

                //Start scanning
                var result = await _scanner.Scan();

                if (IsFinishing || IsDestroyed)
                    return;

                if (result != null)
                {
                    _password.Text = result.Text;
                    SignIn(_signInBtn, null);
                }
            }
            catch (Exception ex)
            {
                await App.Logger.ErrorAsync(ex);
                this.ShowAlert(ex, ToastLength.Short);
            }
        }

        private void OnButtonScanDefaultViewOnClick(object sender, EventArgs e)
        {
            if (!RequestPermissions(CommonPermissionsRequestCode,
                Android.Manifest.Permission.Camera,
                Android.Manifest.Permission.WriteExternalStorage))
                ScanQr();
        }

        protected virtual void GoBack(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        protected abstract void SignIn(object sender, EventArgs e);

        private void HideKeyboard(object sender, EventArgs e)
        {
            HideKeyboard();
        }
    }
}