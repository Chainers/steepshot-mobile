using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Autofac;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class SettingsActivity : BaseActivityWithPresenter<SettingsPresenter>
    {
        private AccountsAdapter _accountsAdapter;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.add_account)] private Button _addButton;
        [InjectView(Resource.Id.dtn_terms_of_service)] private Button _termsButton;
        [InjectView(Resource.Id.tests)] private AppCompatButton _testsButton;
        [InjectView(Resource.Id.nsfw_switch)] private SwitchCompat _nsfwSwitcher;
        [InjectView(Resource.Id.low_switch)] private Switch _lowRatedSwitcher;
        [InjectView(Resource.Id.version_textview)] private TextView _versionText;
        [InjectView(Resource.Id.nsfw_switch_text)] private TextView _nsfwSwitchText;
        [InjectView(Resource.Id.low_switch_text)] private TextView _lowSwitchText;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.accounts_list)] private RecyclerView _accountsList;
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

            _backButton.Visibility = ViewStates.Visible;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Localization.Texts.AppSettingsTitle;

            _viewTitle.Typeface = Style.Semibold;
            _addButton.Typeface = Style.Semibold;
            _versionText.Typeface = Style.Regular;
            _nsfwSwitchText.Typeface = Style.Semibold;
            _lowSwitchText.Typeface = Style.Semibold;
            _termsButton.Typeface = Style.Semibold;

            _accountsList.NestedScrollingEnabled = false;
            _accountsList.SetLayoutManager(new LinearLayoutManager(this));
            _accountsAdapter = new AccountsAdapter();
            _accountsAdapter.AccountsList = accounts;
            _accountsAdapter.DeleteAccount += index =>
            {
                var chainToDelete = _accountsAdapter.AccountsList[index].Chain;
                BasePresenter.User.Delete(_accountsAdapter.AccountsList[index]);
                RemoveChain(chainToDelete);
                _accountsAdapter.NotifyDataSetChanged();
            };
            _accountsAdapter.PickAccount += index =>
            {
                SwitchChain(_accountsAdapter.AccountsList[index]);
            };
            _accountsList.SetAdapter(_accountsAdapter);

            _nsfwSwitcher.CheckedChange += (sender, e) =>
            {
                BasePresenter.User.IsNsfw = _nsfwSwitcher.Checked;
            };

            _lowRatedSwitcher.CheckedChange += (sender, e) =>
            {
                BasePresenter.User.IsLowRated = _lowRatedSwitcher.Checked;
            };

            _nsfwSwitcher.Checked = BasePresenter.User.IsNsfw;
            _lowRatedSwitcher.Checked = BasePresenter.User.IsLowRated;

            if (BasePresenter.User.IsDev || BasePresenter.User.Login.Equals("joseph.kalu"))
            {
                _testsButton.Visibility = ViewStates.Visible;
                _testsButton.Click += StartTestActivity;
            }
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
        public async void AddAccountClick(object sender, EventArgs e)
        {
            await BasePresenter.SwitchChain(BasePresenter.Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem);
            var intent = new Intent(this, typeof(PreSignInActivity));
            StartActivity(intent);
        }

        private void StartTestActivity(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(TestActivity));
            StartActivity(intent);
        }
        
        private void SwitchChain(UserInfo user)
        {
            if (BasePresenter.Chain != user.Chain)
            {
                BasePresenter.SwitchChain(user);
                var i = new Intent(ApplicationContext, typeof(RootActivity));
                i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                StartActivity(i);
            }
        }

        private void RemoveChain(KnownChains chain)
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
                    BasePresenter.SwitchChain(_accountsAdapter.AccountsList.First());
                    var i = new Intent(ApplicationContext, typeof(RootActivity));
                    i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(i);
                    Finish();
                }
                else
                {
                    SetAddButton(accounts.Count);
                }
            }
        }

        private void SetAddButton(int accountsCount)
        {
            if (accountsCount == 2)
                _addButton.Visibility = ViewStates.Gone;
            else
                _addButton.Visibility = ViewStates.Visible;
        }
    }
}
