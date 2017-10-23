using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Autofac;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class SettingsActivity : BaseActivityWithPresenter<SettingsPresenter>
    {
        private UserInfo _steemAcc;
        private UserInfo _golosAcc;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.civ_avatar)] private CircleImageView _avatar;
        [InjectView(Resource.Id.steem_text)] private TextView _steemText;
        [InjectView(Resource.Id.golos_text)] private TextView _golosText;
        [InjectView(Resource.Id.golosView)] private RelativeLayout _golosView;
        [InjectView(Resource.Id.steemView)] private RelativeLayout _steemView;
        [InjectView(Resource.Id.add_account)] private AppCompatButton _addButton;
        [InjectView(Resource.Id.nsfw_switch)] private SwitchCompat _nsfwSwitcher;
        [InjectView(Resource.Id.low_switch)] private SwitchCompat _lowRatedSwitcher;
        [InjectView(Resource.Id.version_textview)] private TextView _versionText;
        [InjectView(Resource.Id.tests)] private AppCompatButton _testsButton;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_settings);
            Cheeseknife.Inject(this);

            var appInfoService = AppSettings.Container.Resolve<IAppInfo>();
            _versionText.Text = Localization.Messages.AppVersion(appInfoService.GetAppVersion(), appInfoService.GetBuildVersion());
            var accounts = BasePresenter.User.GetAllAccounts();

            SetAddButton(accounts.Count);

            _steemAcc = accounts.FirstOrDefault(a => a.Chain == KnownChains.Steem);
            _golosAcc = accounts.FirstOrDefault(a => a.Chain == KnownChains.Golos);

            if (_steemAcc != null)
            {
                _steemText.Text = _steemAcc.Login;
                //Picasso.With(ApplicationContext).Load(steemAcc.).Into(stee);
            }
            else
                _steemView.Visibility = ViewStates.Gone;

            if (_golosAcc != null)
            {
                _golosText.Text = _golosAcc.Login;
                //Picasso.With(ApplicationContext).Load(steemAcc.).Into(stee);
            }
            else
                _golosView.Visibility = ViewStates.Gone;

            _nsfwSwitcher.CheckedChange += (sender, e) =>
            {
                BasePresenter.User.IsNsfw = _nsfwSwitcher.Checked;
            };

            _lowRatedSwitcher.CheckedChange += (sender, e) =>
            {
                BasePresenter.User.IsLowRated = _lowRatedSwitcher.Checked;
            };

            HighlightView();
            _nsfwSwitcher.Checked = BasePresenter.User.IsNsfw;
            _lowRatedSwitcher.Checked = BasePresenter.User.IsLowRated;

            if (BasePresenter.User.IsDev || BasePresenter.User.Login.Equals("joseph.kalu"))
            {
                _testsButton.Visibility = ViewStates.Visible;
                _testsButton.Click += StartTestActivity;
            }

            LoadAvatar();
        }

        protected override void CreatePresenter()
        {
            _presenter = new SettingsPresenter();
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        [InjectOnClick(Resource.Id.dtn_terms_of_service)]
        public void TermsOfServiceClick(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("https://steepshot.org/terms-of-service");
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.add_account)]
        public void AddAccountClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(PreSignInActivity));
            intent.PutExtra(PreSignInActivity.ChainExtraPath, (int)(BasePresenter.Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem));
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.golosView)]
        public async void GolosViewClick(object sender, EventArgs e)
        {
            await SwitchChain(_golosAcc);
        }

        [InjectOnClick(Resource.Id.steemView)]
        public async void SteemViewClick(object sender, EventArgs e)
        {
            await SwitchChain(_steemAcc);
        }

        [InjectOnClick(Resource.Id.remove_steem)]
        public async void RemoveSteem(object sender, EventArgs e)
        {
            BasePresenter.User.Delete(_steemAcc);
            _steemView.Visibility = ViewStates.Gone;
            await RemoveChain(KnownChains.Steem);
        }

        [InjectOnClick(Resource.Id.remove_golos)]
        public async void RemoveGolos(object sender, EventArgs e)
        {
            BasePresenter.User.Delete(_golosAcc);
            _golosView.Visibility = ViewStates.Gone;
            await RemoveChain(KnownChains.Golos);
        }


        private void StartTestActivity(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(TestActivity));
            StartActivity(intent);
        }

        private async void LoadAvatar()
        {
            var info = await _presenter.TryGetUserInfo();

            if (info != null && info.Success)
            {
                if (!string.IsNullOrEmpty(info.Result.ProfileImage))
                    Picasso.With(ApplicationContext).Load(info.Result.ProfileImage).Into(_avatar);
            }
            else
            {
                ShowAlert(info);
            }
        }

        private async Task SwitchChain(UserInfo user)
        {
            if (BasePresenter.Chain != user.Chain)
            {
                await BasePresenter.SwitchChain(user);
                var i = new Intent(ApplicationContext, typeof(RootActivity));
                i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(i);
            }
        }

        private async Task RemoveChain(KnownChains chain)
        {
            var accounts = BasePresenter.User.GetAllAccounts();
            if (accounts.Count == 0)
            {
                var i = new Intent(ApplicationContext, typeof(GuestActivity));
                i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(i);
                Finish();
            }
            else
            {
                if (BasePresenter.Chain == chain)
                {
                    await BasePresenter.SwitchChain(chain == KnownChains.Steem ? _golosAcc : _steemAcc);
                    var i = new Intent(ApplicationContext, typeof(RootActivity));
                    i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(i);
                    Finish();
                }
                else
                {
                    HighlightView();
                    SetAddButton(accounts.Count);
                }
            }
        }

        private void HighlightView()
        {
            if (BasePresenter.Chain == KnownChains.Steem)
                _steemView.SetBackgroundColor(Color.Cyan);
            else
                _golosView.SetBackgroundColor(Color.Cyan);
        }

        private void SetAddButton(int accountsCount)
        {
            if (accountsCount == 2)
                _addButton.Visibility = ViewStates.Gone;
        }

    }
}
