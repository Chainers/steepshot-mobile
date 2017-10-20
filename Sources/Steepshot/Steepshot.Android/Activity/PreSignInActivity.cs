using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
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
    public sealed class PreSignInActivity : BaseActivityWithPresenter<PreSignInPresenter>
    {
        private KnownChains _newChain;
        private int _clickCount;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.input_username)] private EditText _username;
        [InjectView(Resource.Id.chain_switch)] private SwitchCompat _switcher;
        [InjectView(Resource.Id.login_label)] private TextView _loginLabel;
        [InjectView(Resource.Id.steem_logo)] private ImageView _steemLogo;
        [InjectView(Resource.Id.golos_logo)] private ImageView _golosLogo;
        [InjectView(Resource.Id.dev_switch)] private SwitchCompat _devSwitcher;
        [InjectView(Resource.Id.ic_logo)] private ImageView _logo;
#pragma warning restore 0649

        protected override void CreatePresenter()
        {
            _presenter = new PreSignInPresenter();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_pre_sign_in);
            Cheeseknife.Inject(this);
#if DEBUG
            _username.Text = "joseph.kalu";
#endif
            _newChain = (KnownChains)Intent.GetIntExtra("newChain", (int)KnownChains.None);
            if (_newChain != KnownChains.None)
            {
                _switcher.Visibility = ViewStates.Gone;
                _steemLogo.Visibility = ViewStates.Gone;
                _golosLogo.Visibility = ViewStates.Gone;
                BasePresenter.SwitchChain(_newChain);
            }

            _switcher.Checked = BasePresenter.Chain == KnownChains.Steem;
            _switcher.CheckedChange += (sender, e) =>
            {
                BasePresenter.SwitchChain(e.IsChecked ? KnownChains.Steem : KnownChains.Golos);
                SetLabelsText();
            };

            _devSwitcher.Checked = AppSettings.IsDev;
            _devSwitcher.CheckedChange += (sender, e) =>
            {
                BasePresenter.SwitchChain(e.IsChecked);
            };

            SetLabelsText();
        }

        [InjectOnClick(Resource.Id.ic_logo)]
        private void Logo_Click(object sender, EventArgs e)
        {
            _clickCount++;
            if (_clickCount == 5)
            {
                _devSwitcher.Visibility = ViewStates.Visible;
                _clickCount = 0;
            }
            else
            {
                _devSwitcher.Visibility = ViewStates.Gone;
            }
        }

        [InjectOnClick(Resource.Id.reg_button)]
        private void RegistrationClick(object sender, EventArgs e)
        {
            var url = BasePresenter.Chain == KnownChains.Steem ? Constants.SteemitRegUrl : Constants.GolosRegUrl;
            var uri = Android.Net.Uri.Parse(url);
            Intent browserIntent = new Intent(Intent.ActionView, uri);
            StartActivity(browserIntent);
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

        [InjectOnClick(Resource.Id.pre_sign_in_btn)]
        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            var login = _username.Text?.ToLower().Trim();

            if (string.IsNullOrEmpty(login))
            {
                ShowAlert(Localization.Errors.EmptyLogin, ToastLength.Short);
                return;
            }

            _spinner.Visibility = ViewStates.Visible;
            ((AppCompatButton)sender).Visibility = ViewStates.Invisible;
            ((AppCompatButton)sender).Enabled = false;

            var response = await _presenter.TryGetAccountInfo(login);
            if (response != null && response.Success)
            {
                var intent = new Intent(this, typeof(SignInActivity));
                intent.PutExtra("login", login);
                intent.PutExtra("avatar_url", response.Result.ProfileImage);
                intent.PutExtra("newChain", (int)_newChain);
                _newChain = KnownChains.None;
                StartActivity(intent);
            }
            else
            {
                ShowAlert(response);
            }
            _spinner.Visibility = ViewStates.Invisible;
            ((AppCompatButton)sender).Visibility = ViewStates.Visible;
            ((AppCompatButton)sender).Enabled = true;
        }

        private void SetLabelsText()
        {
            _loginLabel.Text = Localization.Messages.LoginMsg(BasePresenter.Chain);
        }
    }
}
