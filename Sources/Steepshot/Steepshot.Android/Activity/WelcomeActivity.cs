using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
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

namespace Steepshot.Activity
{
    [Activity(NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
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
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_welcome);
            Cheeseknife.Inject(this);

            var font = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Regular.ttf");
            var semibold_font = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Semibold.ttf");

            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.N)
            {
                _termsTextView.TextFormatted = Html.FromHtml(
                    $"By pressing any of the buttons you agree with our <a href=\"{Constants.Tos}\">Terms of Use</a> and <a href=\"{Constants.Pp}\">Privacy policy</a>", FromHtmlOptions.ModeLegacy
                );
            }
            else
            {
                _termsTextView.TextFormatted = Html.FromHtml(
                    $"By pressing any of the buttons you agree with our <a href=\"{Constants.Tos}\">Terms of Use</a> and <a href=\"{Constants.Pp}\">Privacy policy</a>"
                );
            }
            _termsTextView.MovementMethod = new LinkMovementMethod();

            _termsTextView.Typeface = font;
            _steemLogin.Typeface = semibold_font;
            _regButton.Typeface = semibold_font;

            _steemLogin.Text = Localization.Texts.SignInButtonText;
            _regButton.Text = Localization.Texts.CreateButtonText;
            _devSwitcher.Checked = AppSettings.IsDev;
            _devSwitcher.CheckedChange += (sender, e) =>
            {
                BasePresenter.SwitchChain(e.IsChecked);
            };
        }

        [InjectOnClick(Resource.Id.steem_login)]
        private async void SteemLogin(object sender, EventArgs e)
        {
            _steemLoader.Visibility = ViewStates.Visible;
            _steemLogin.Enabled = false;
            _steemLogin.Text = string.Empty;
            await PickChain(KnownChains.Steem);
            _steemLoader.Visibility = ViewStates.Gone;
            _steemLogin.Enabled = true;
            _steemLogin.Text = Localization.Texts.SignInButtonText;
        }

        [InjectOnClick(Resource.Id.golos_login)]
        private async void GolosLogin(object sender, EventArgs e)
        {
            _golosLoder.Visibility = ViewStates.Visible;
            _golosLogin.Enabled = false;
            await PickChain(KnownChains.Golos);
            _golosLoder.Visibility = ViewStates.Gone;
            _golosLogin.Enabled = true;
        }

        [InjectOnClick(Resource.Id.reg_button)]
        private void RegistrationClick(object sender, EventArgs e)
        {
            var url = /* BasePresenter.Chain == KnownChains.Steem ?*/Constants.SteemitRegUrl;/* : Constants.GolosRegUrl;*/
            var uri = Android.Net.Uri.Parse(url);
            Intent browserIntent = new Intent(Intent.ActionView, uri);
            StartActivity(browserIntent);
        }

        private async Task PickChain(KnownChains chain)
        {
            await Task.Run(() => {
                if (BasePresenter.Chain != chain)
                    BasePresenter.SwitchChain(chain);
            });
            var intent = new Intent(this, typeof(PreSignInActivity));
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.steepshot_logo)]
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
    }
}
