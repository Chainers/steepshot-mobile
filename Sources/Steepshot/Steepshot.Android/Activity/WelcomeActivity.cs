using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class WelcomeActivity : BaseActivity
    {
        private Dialog _regActionsDialog;
        private RegistrationType registrationType;
        private int _clickCount;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.steem_login)] private Button _steemLogin;
        [BindView(Resource.Id.golos_login)] private Button _golosLogin;
        [BindView(Resource.Id.reg_button)] private Button _regButton;
        [BindView(Resource.Id.dev_switch)] private SwitchCompat _devSwitcher;
        [BindView(Resource.Id.steem_loading_spinner)] private ProgressBar _steemLoader;
        [BindView(Resource.Id.golos_loading_spinner)] private ProgressBar _golosLoder;
        [BindView(Resource.Id.terms)] private TextView _termsTextView;
        [BindView(Resource.Id.steepshot_logo)] private ImageView _steepshotLogo;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_welcome);
            Cheeseknife.Bind(this);

            var msg = AppSettings.LocalizationManager.GetText(LocalizationKeys.TitleForAcceptToS);
            _termsTextView.TextFormatted = Build.VERSION.SdkInt >= Build.VERSION_CODES.N
                ? Html.FromHtml(msg, FromHtmlOptions.ModeLegacy)
                : Html.FromHtml(msg);

            _termsTextView.MovementMethod = new LinkMovementMethod();

            _termsTextView.Typeface = Style.Regular;
            _steemLogin.Typeface = Style.Semibold;
            _golosLogin.Typeface = Style.Semibold;
            _regButton.Typeface = Style.Semibold;

            _steemLogin.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SignInButtonText, "Steem");
            _golosLogin.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SignInButtonText, "Golos");
            _regButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.CreateButtonText);
            _devSwitcher.Checked = AppSettings.IsDev;
            _devSwitcher.CheckedChange += OnDevSwitcherOnCheckedChange;

            _steemLogin.Click += SteemLogin;
            _golosLogin.Click += GolosLogin;
            _regButton.Click += RegistrationClick;
            _steepshotLogo.Click += Logo_Click;

            _regActionsDialog = new BottomSheetDialog(this);
            _regActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        private async void SteemLogin(object sender, EventArgs e)
        {
            _steemLoader.Visibility = ViewStates.Visible;
            _steemLogin.Enabled = false;
            _steemLogin.Text = string.Empty;

            await PickChain(KnownChains.Steem);
            if (IsFinishing || IsDestroyed)
                return;

            _steemLoader.Visibility = ViewStates.Gone;
            _steemLogin.Enabled = true;
            _steemLogin.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SignInButtonText, "Steem");
        }

        private async void GolosLogin(object sender, EventArgs e)
        {
            _golosLoder.Visibility = ViewStates.Visible;
            _golosLogin.Enabled = false;
            _golosLogin.Text = string.Empty;

            await PickChain(KnownChains.Golos);
            if (IsFinishing || IsDestroyed)
                return;

            _golosLoder.Visibility = ViewStates.Gone;
            _golosLogin.Enabled = true;
            _golosLogin.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SignInButtonText, "Golos");
        }

        private void RegistrationClick(object sender, EventArgs e)
        {
            var inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_registration_alert, null))
            {
                var title = dialogView.FindViewById<TextView>(Resource.Id.registration_title);
                var steemit = dialogView.FindViewById<Button>(Resource.Id.steemit_btn);
                var blocktrades = dialogView.FindViewById<Button>(Resource.Id.blocktrades_btn);
                var steemcreate = dialogView.FindViewById<Button>(Resource.Id.steemcreate_btn);
                var use = dialogView.FindViewById<Button>(Resource.Id.use_registration);
                var close = dialogView.FindViewById<Button>(Resource.Id.close_btn);

                title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RegistrationWith);
                title.Typeface = Style.Semibold;

                use.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Use);
                close.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Close);
                close.Typeface = Style.Semibold;

                steemit.Selected = true;
                registrationType = RegistrationType.Steemit;

                steemit.SetCompoundDrawablesWithIntrinsicBounds(SetupLogo(Resource.Drawable.ic_steem), null, null, null);
                blocktrades.SetCompoundDrawablesWithIntrinsicBounds(SetupLogo(Resource.Drawable.ic_blocktrade), null, null, null);
                steemcreate.SetCompoundDrawablesWithIntrinsicBounds(SetupLogo(Resource.Drawable.ic_steemcreate), null, null, null);

                steemit.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RegisterThroughSteemit);
                blocktrades.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RegisterThroughBlocktrades);
                steemcreate.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RegisterThroughSteemCreate);

                use.Click += (o, args) =>
                {
                    _regActionsDialog.Dismiss();
                    OnUse();
                };

                close.Click += (o, args) =>
                {
                    _regActionsDialog.Dismiss();
                };

                steemit.Click += (o, args) =>
                {
                    blocktrades.Selected = steemcreate.Selected = false;
                    steemit.Selected = true;
                    registrationType = RegistrationType.Steemit;
                };

                blocktrades.Click += (o, args) =>
                {
                    steemit.Selected = steemcreate.Selected = false;
                    blocktrades.Selected = true;
                    registrationType = RegistrationType.Blocktrades;
                };

                steemcreate.Click += (o, args) =>
                {
                    steemit.Selected = blocktrades.Selected = false;
                    steemcreate.Selected = true;
                    registrationType = RegistrationType.SteemCreate;
                };

                _regActionsDialog.SetContentView(dialogView);
                var bottomSheet = _regActionsDialog.Window.FindViewById(Resource.Id.design_bottom_sheet);
                bottomSheet.SetBackgroundColor(Color.Transparent);
                _regActionsDialog.Show();

                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private BitmapDrawable SetupLogo(int drawable)
        {
            var logoSide = (int)BitmapUtils.DpToPixel(80, Resources);
            var originalImage = BitmapFactory.DecodeResource(Resources, drawable);
            var scaledBitmap = Bitmap.CreateScaledBitmap(originalImage, logoSide, logoSide, true);
            return new BitmapDrawable(Resources, scaledBitmap);
        }

        private void OnUse()
        {
            Android.Net.Uri uri;

            switch (registrationType)
            {
                case RegistrationType.Steemit:
                    uri = Android.Net.Uri.Parse(Constants.SteemitRegUrl);
                    break;
                case RegistrationType.Blocktrades:
                    uri = Android.Net.Uri.Parse(Constants.BlocktradesRegUrl);
                    break;
                default:
                    uri = Android.Net.Uri.Parse(Constants.SteemCreateRegUrl);
                    break;
            }

            var browserIntent = new Intent(Intent.ActionView, uri);
            StartActivity(browserIntent);
        }

        private void Logo_Click(object sender, EventArgs e)
        {
            _clickCount++;
            if (_clickCount == 5)
            {
                _devSwitcher.Visibility = ViewStates.Visible;
                _clickCount = 0;
            }
            else
                _devSwitcher.Visibility = ViewStates.Gone;
        }

        private async void OnDevSwitcherOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            await BasePresenter.SwitchChain(e.IsChecked);
        }

        private async Task PickChain(KnownChains chain)
        {
            if (BasePresenter.Chain != chain)
            {
                await BasePresenter.SwitchChain(chain);
                if (IsFinishing || IsDestroyed)
                    return;
            }

            var intent = new Intent(this, typeof(PreSignInActivity));
            StartActivity(intent);
        }
    }
}
