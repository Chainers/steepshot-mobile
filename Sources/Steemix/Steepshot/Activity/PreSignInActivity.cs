using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace Steepshot
{
    [Activity(NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PreSignInActivity : BaseActivity, PreSignInView
    {
        PreSignInPresenter presenter;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar spinner;
        [InjectView(Resource.Id.input_username)] private EditText username;
        [InjectView(Resource.Id.chain_switch)] private SwitchCompat switcher;
        [InjectView(Resource.Id.login_label)] private TextView loginLabel;
        [InjectView(Resource.Id.steem_logo)] private ImageView steem_logo;
        [InjectView(Resource.Id.golos_logo)] private ImageView golos_logo;
        [InjectView(Resource.Id.dev_switch)] private SwitchCompat dev_switcher;
        [InjectView(Resource.Id.ic_logo)] private ImageView logo;
#pragma warning restore 0649

        private KnownChains _newChain;
        private int _clickCount;
        protected override void CreatePresenter()
        {
            presenter = new PreSignInPresenter(this);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_pre_sign_in);
            Cheeseknife.Inject(this);
#if DEBUG
            username.Text = "joseph.kalu";
#endif
            _newChain = (KnownChains)Intent.GetIntExtra("newChain", (int)KnownChains.None);
            if (_newChain == KnownChains.Steem || _newChain == KnownChains.Golos)
            {
                switcher.Visibility = ViewStates.Gone;
                steem_logo.Visibility = ViewStates.Gone;
                golos_logo.Visibility = ViewStates.Gone;
                User.Chain = _newChain;

                BasePresenter.SwitchChain();
            }

            switcher.Checked = User.Chain == KnownChains.Steem;
            switcher.CheckedChange += (sender, e) =>
            {
                User.Chain = e.IsChecked ? KnownChains.Steem : KnownChains.Golos;
                BasePresenter.SwitchChain();
                SetLabelsText();
            };

            dev_switcher.Checked = User.IsDev;
            dev_switcher.CheckedChange += (sender, e) =>
            {
                User.IsDev = e.IsChecked;
                BasePresenter.SwitchChain();
            };

            SetLabelsText();
        }

        [InjectOnClick(Resource.Id.ic_logo)]
        private void Logo_Click(object sender, System.EventArgs e)
        {
            _clickCount++;
            if (_clickCount == 5)
            {
                dev_switcher.Visibility = ViewStates.Visible;
                _clickCount = 0;
            }
            else
            {
                dev_switcher.Visibility = ViewStates.Gone;
            }
        }

        protected override void OnDestroy()
        {
            if (_newChain == KnownChains.Steem || _newChain == KnownChains.Golos)
            {
                User.Chain = _newChain;
                BasePresenter.SwitchChain();
            }
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        [InjectOnClick(Resource.Id.sign_in_btn)]
        private async void SignInBtn_Click(object sender, System.EventArgs e)
        {
            try
            {
                var login = username.Text.ToLower();

                if (string.IsNullOrEmpty(login))
                {
                    Toast.MakeText(this, "Invalid credentials", ToastLength.Short).Show();
                    return;
                }

                spinner.Visibility = ViewStates.Visible;
                ((AppCompatButton)sender).Visibility = ViewStates.Invisible;
                ((AppCompatButton)sender).Enabled = false;

                if (string.IsNullOrEmpty(login))
                    return;

                var response = await presenter.GetAccountInfo(login);

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
                        spinner.Visibility = ViewStates.Invisible;
                        ((AppCompatButton)sender).Visibility = ViewStates.Visible;
                        ((AppCompatButton)sender).Enabled = true;
                    }
                }
                else
                {
                    ShowAlert(Resource.String.error_connect_to_server);
                    spinner.Visibility = ViewStates.Invisible;
                    ((AppCompatButton)sender).Visibility = ViewStates.Visible;
                    ((AppCompatButton)sender).Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
        }

        private void SetLabelsText()
        {
            loginLabel.Text = $"Log in with your {User.Chain} Account";
        }
    }
}
