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
using Steepshot.Core.Utils;
using Steepshot.Presenter;

using Steepshot.View;

namespace Steepshot.Activity
{
    [Activity(NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PreSignInActivity : BaseActivity, IPreSignInView
    {
        PreSignInPresenter _presenter;

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

        private KnownChains _newChain;
        private int _clickCount;
        protected override void CreatePresenter()
        {
            _presenter = new PreSignInPresenter(this);
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

            _devSwitcher.Checked = BasePresenter.User.IsDev;
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

        protected override void OnDestroy()
        {
            if (_newChain != KnownChains.None)
            {
                BasePresenter.SwitchChain(_newChain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem);
            }
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        [InjectOnClick(Resource.Id.sign_in_btn)]
        private async void SignInBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var login = _username.Text?.ToLower();

                if (string.IsNullOrEmpty(login))
                {
                    Toast.MakeText(this, "Invalid credentials", ToastLength.Short).Show();
                    return;
                }

                _spinner.Visibility = ViewStates.Visible;
                ((AppCompatButton)sender).Visibility = ViewStates.Invisible;
                ((AppCompatButton)sender).Enabled = false;

                var response = await _presenter.GetAccountInfo(login);

                if (response != null)
                {
                    if (response.Success)
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
                        ShowAlert(response.Errors[0]);
                        _spinner.Visibility = ViewStates.Invisible;
                        ((AppCompatButton)sender).Visibility = ViewStates.Visible;
                        ((AppCompatButton)sender).Enabled = true;
                    }
                }
                else
                {
                    ShowAlert(Resource.String.error_connect_to_server);
                    _spinner.Visibility = ViewStates.Invisible;
                    ((AppCompatButton)sender).Visibility = ViewStates.Visible;
                    ((AppCompatButton)sender).Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
            }
        }

        private void SetLabelsText()
        {
            _loginLabel.Text = $"Log in with your {BasePresenter.Chain} Account";
        }
    }
}
