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
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class WelcomeActivity : BaseActivity
    {
        private int _clickCount;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.steem_login)] private Button _steemLogin;
        [InjectView(Resource.Id.golos_login)] private ImageButton _golosLogin;
        [InjectView(Resource.Id.reg_button)] private Button _regButton;
        [InjectView(Resource.Id.dev_switch)] private SwitchCompat _devSwitcher;
        [InjectView(Resource.Id.steem_loading_spinner)] private ProgressBar _steemLoader;
        [InjectView(Resource.Id.golos_loading_spinner)] private ProgressBar _golosLoder;
        [InjectView(Resource.Id.terms)] private TextView _termsTextView;
        [InjectView(Resource.Id.steepshot_logo)] private ImageView _steepshotLogo;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_welcome);
            Cheeseknife.Inject(this);

            _termsTextView.TextFormatted = Build.VERSION.SdkInt >= Build.VERSION_CODES.N
                ? Html.FromHtml(Localization.Messages.TitleForAcceptToS, FromHtmlOptions.ModeLegacy)
                : Html.FromHtml(Localization.Messages.TitleForAcceptToS);

            _termsTextView.MovementMethod = new LinkMovementMethod();

            _termsTextView.Typeface = Style.Regular;
            _steemLogin.Typeface = Style.Semibold;
            _regButton.Typeface = Style.Semibold;

            _steemLogin.Text = Localization.Texts.SignInButtonText;
            _regButton.Text = Localization.Texts.CreateButtonText;
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
            _steemLogin.Text = Localization.Texts.SignInButtonText;
        }

        private async void GolosLogin(object sender, EventArgs e)
        {
            _golosLoder.Visibility = ViewStates.Visible;
            _golosLogin.Enabled = false;

            await PickChain(KnownChains.Golos);
            if (IsFinishing || IsDestroyed)
                return;

            _golosLoder.Visibility = ViewStates.Gone;
            _golosLogin.Enabled = true;
        }

        private void RegistrationClick(object sender, EventArgs e)
        {
            var url = BasePresenter.Chain == KnownChains.Golos
                ? Constants.GolosRegUrl
                : Constants.SteemitRegUrl;

            var uri = Android.Net.Uri.Parse(url);
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
