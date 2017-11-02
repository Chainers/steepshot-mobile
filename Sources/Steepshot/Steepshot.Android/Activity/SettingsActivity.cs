using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class SettingsActivity : BaseActivity
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

            var appInfoService = AppSettings.AppInfo;
            _versionText.Text = Localization.Messages.AppVersion(appInfoService.GetAppVersion(), appInfoService.GetBuildVersion());
            var accounts = BasePresenter.User.GetAllAccounts();

            SetAddButton(accounts.Count);

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBackClick;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Localization.Texts.AppSettingsTitle;

            _viewTitle.Typeface = Style.Semibold;
            _addButton.Typeface = Style.Semibold;
            _versionText.Typeface = Style.Regular;
            _nsfwSwitchText.Typeface = Style.Semibold;
            _lowSwitchText.Typeface = Style.Semibold;
            _termsButton.Typeface = Style.Semibold;
            _termsButton.Click += TermsOfServiceClick;

            _addButton.Text = Localization.Texts.AddAccountText;
            _addButton.Click += AddAccountClick;

            _accountsAdapter = new AccountsAdapter();
            _accountsAdapter.AccountsList = accounts;
            _accountsAdapter.DeleteAccount += OnAdapterDeleteAccount;
            _accountsAdapter.PickAccount += OnAdapterPickAccount;

            _accountsList.NestedScrollingEnabled = false;
            _accountsList.SetLayoutManager(new LinearLayoutManager(this));
            _accountsList.SetAdapter(_accountsAdapter);

            _nsfwSwitcher.CheckedChange += OnNsfwSwitcherOnCheckedChange;

            _lowRatedSwitcher.CheckedChange += OnLowRatedSwitcherOnCheckedChange;

            _nsfwSwitcher.Checked = BasePresenter.User.IsNsfw;
            _lowRatedSwitcher.Checked = BasePresenter.User.IsLowRated;

            //for tests
            if (BasePresenter.User.IsDev || BasePresenter.User.Login.Equals("joseph.kalu"))
            {
                _testsButton.Visibility = ViewStates.Visible;
                _testsButton.Click += StartTestActivity;
            }
        }

        private void OnLowRatedSwitcherOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            BasePresenter.User.IsLowRated = _lowRatedSwitcher.Checked;
        }

        private void OnNsfwSwitcherOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            BasePresenter.User.IsNsfw = _nsfwSwitcher.Checked;
        }

        private void OnAdapterPickAccount(int index)
        {
            if (_accountsAdapter.AccountsList.Count <= index)
                return;
            SwitchChain(_accountsAdapter.AccountsList[index]);
        }

        private void OnAdapterDeleteAccount(int index)
        {
            if (_accountsAdapter.AccountsList.Count <= index)
                return;

            var acc = _accountsAdapter.AccountsList[index];
            var chainToDelete = acc.Chain;
            BasePresenter.User.Delete(acc);
            RemoveChain(chainToDelete);
            _accountsAdapter.NotifyDataSetChanged();
        }

        private void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private void TermsOfServiceClick(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("https://steepshot.org/terms-of-service");
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        private async void AddAccountClick(object sender, EventArgs e)
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
