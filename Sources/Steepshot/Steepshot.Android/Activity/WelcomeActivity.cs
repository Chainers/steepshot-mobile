using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class WelcomeActivity : BaseActivity
    {
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
            var intent = new Intent(this, typeof(RegistrationActivity));
            StartActivity(intent);
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
