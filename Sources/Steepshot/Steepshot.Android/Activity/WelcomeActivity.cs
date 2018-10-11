using System;
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
        [BindView(Resource.Id.steem_loading_spinner)] private ProgressBar _steemLoader;
        [BindView(Resource.Id.golos_loading_spinner)] private ProgressBar _golosLoder;
        [BindView(Resource.Id.terms)] private TextView _termsTextView;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_welcome);
            Cheeseknife.Bind(this);

            var msg = App.Localization.GetText(LocalizationKeys.TitleForAcceptToS);
            _termsTextView.TextFormatted = Build.VERSION.SdkInt >= Build.VERSION_CODES.N
                ? Html.FromHtml(msg, FromHtmlOptions.ModeLegacy)
                : Html.FromHtml(msg);

            _termsTextView.MovementMethod = new LinkMovementMethod();

            _termsTextView.Typeface = Style.Regular;
            _steemLogin.Typeface = Style.Semibold;
            _golosLogin.Typeface = Style.Semibold;
            _regButton.Typeface = Style.Semibold;

            _steemLogin.Text = App.Localization.GetText(LocalizationKeys.SignInButtonText, "Steem");
            _golosLogin.Text = App.Localization.GetText(LocalizationKeys.SignInButtonText, "Golos");
            _regButton.Text = App.Localization.GetText(LocalizationKeys.CreateButtonText);

            _steemLogin.Click += SteemLogin;
            _golosLogin.Click += GolosLogin;
            _regButton.Click += RegistrationClick;
        }
        
        private async void SteemLogin(object sender, EventArgs e)
        {
            _steemLoader.Visibility = ViewStates.Visible;
            _steemLogin.Enabled = false;
            _steemLogin.Text = string.Empty;

            PickChain(KnownChains.Steem);

            _steemLoader.Visibility = ViewStates.Gone;
            _steemLogin.Enabled = true;
            _steemLogin.Text = App.Localization.GetText(LocalizationKeys.SignInButtonText, "Steem");
        }

        private async void GolosLogin(object sender, EventArgs e)
        {
            _golosLoder.Visibility = ViewStates.Visible;
            _golosLogin.Enabled = false;
            _golosLogin.Text = string.Empty;

            PickChain(KnownChains.Golos);
            _golosLoder.Visibility = ViewStates.Gone;
            _golosLogin.Enabled = true;
            _golosLogin.Text = App.Localization.GetText(LocalizationKeys.SignInButtonText, "Golos");
        }

        private void RegistrationClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(RegistrationActivity));
            StartActivity(intent);
        }
        
        private void PickChain(KnownChains chain)
        {
            App.MainChain = chain;
            var intent = new Intent(this, typeof(PreSignInActivity));
            StartActivity(intent);
        }
    }
}
