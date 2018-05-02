using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.OneSignal;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Steepshot.Core.Models.Requests;
using System.Threading.Tasks;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class SettingsActivity : BaseActivity
    {
        private AccountsAdapter _accountsAdapter;
        private bool _lowRatedChanged;
        private bool _nsfwChanged;
        private PushSettings PushSettings;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.add_account)] private Button _addButton;
        [BindView(Resource.Id.dtn_terms_of_service)] private Button _termsButton;
        [BindView(Resource.Id.tests)] private AppCompatButton _testsButton;
        [BindView(Resource.Id.btn_guide)] private Button _guideButton;
        [BindView(Resource.Id.nsfw_switch)] private SwitchCompat _nsfwSwitcher;
        [BindView(Resource.Id.low_switch)] private SwitchCompat _lowRatedSwitcher;
        [BindView(Resource.Id.version_textview)] private TextView _versionText;
        [BindView(Resource.Id.nsfw_switch_text)] private TextView _nsfwSwitchText;
        [BindView(Resource.Id.low_switch_text)] private TextView _lowSwitchText;
        [BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.accounts_list)] private RecyclerView _accountsList;
        [BindView(Resource.Id.add_account_loading_spinner)] private ProgressBar _addAccountLoader;
        [BindView(Resource.Id.header_text)] private TextView _notificationSettings;
        [BindView(Resource.Id.post_upvotes)] private TextView _notificationUpvotes;
        [BindView(Resource.Id.post_upvotes_switch)] private SwitchCompat _notificationUpvotesSwitch;
        [BindView(Resource.Id.comments_upvotes)] private TextView _notificationCommentsUpvotes;
        [BindView(Resource.Id.comments_upvotes_switch)] private SwitchCompat _notificationCommentsUpvotesSwitch;
        [BindView(Resource.Id.following)] private TextView _notificationFollowing;
        [BindView(Resource.Id.following_switch)] private SwitchCompat _notificationFollowingSwitch;
        [BindView(Resource.Id.comments)] private TextView _notificationComments;
        [BindView(Resource.Id.comments_switch)] private SwitchCompat _notificationCommentsSwitch;
        [BindView(Resource.Id.posting)] private TextView _notificationPosting;
        [BindView(Resource.Id.posting_switch)] private SwitchCompat _notificationPostingSwitch;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_settings);
            Cheeseknife.Bind(this);

            var appInfoService = AppSettings.AppInfo;
            var accounts = AppSettings.User.GetAllAccounts();

            _viewTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AppSettingsTitle);
            _nsfwSwitchText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ShowNsfw);
            _lowSwitchText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ShowLowRated);
            _versionText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AppVersion2, appInfoService.GetAppVersion(), appInfoService.GetBuildVersion());
            _addButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AddAccountText);
            _guideButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Guidelines);
            _termsButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ToS);
            _notificationSettings.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NotificationSettings);
            _notificationUpvotes.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NotificationPostUpvotes);
            _notificationCommentsUpvotes.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NotificationCommentsUpvotes);
            _notificationFollowing.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NotificationFollow);
            _notificationComments.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NotificationComment);
            _notificationPosting.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NotificationPosting);

            SetAddButton(accounts.Count);

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBackClick;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;

            _viewTitle.Typeface = Style.Semibold;
            _addButton.Typeface = Style.Semibold;
            _versionText.Typeface = Style.Regular;
            _nsfwSwitchText.Typeface = Style.Semibold;
            _lowSwitchText.Typeface = Style.Semibold;
            _termsButton.Typeface = Style.Semibold;
            _notificationSettings.Typeface = Style.Semibold;
            _notificationUpvotes.Typeface = Style.Semibold;
            _notificationCommentsUpvotes.Typeface = Style.Semibold;
            _notificationFollowing.Typeface = Style.Semibold;
            _notificationComments.Typeface = Style.Semibold;
            _notificationPosting.Typeface = Style.Semibold;
            _termsButton.Click += TermsOfServiceClick;
            _guideButton.Typeface = Style.Semibold;
            _guideButton.Click += GuideClick;

            _addButton.Click += AddAccountClick;

            _accountsAdapter = new AccountsAdapter();
            _accountsAdapter.AccountsList = accounts;
            _accountsAdapter.DeleteAccount += OnAdapterDeleteAccount;
            _accountsAdapter.PickAccount += OnAdapterPickAccount;

            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Lollipop)
                _accountsList.NestedScrollingEnabled = false;
            _accountsList.SetLayoutManager(new LinearLayoutManager(this));
            _accountsList.SetAdapter(_accountsAdapter);

            _nsfwSwitcher.Checked = AppSettings.User.IsNsfw;
            _lowRatedSwitcher.Checked = AppSettings.User.IsLowRated;

            PushSettings = AppSettings.User.PushSettings;
            _notificationUpvotesSwitch.Checked = PushSettings.HasFlag(PushSettings.Upvote);
            _notificationCommentsUpvotesSwitch.Checked = PushSettings.HasFlag(PushSettings.UpvoteComment);
            _notificationFollowingSwitch.Checked = PushSettings.HasFlag(PushSettings.Follow);
            _notificationCommentsSwitch.Checked = PushSettings.HasFlag(PushSettings.Comment);
            _notificationPostingSwitch.Checked = PushSettings.HasFlag(PushSettings.User);

            _nsfwSwitcher.CheckedChange += OnNsfwSwitcherOnCheckedChange;
            _lowRatedSwitcher.CheckedChange += OnLowRatedSwitcherOnCheckedChange;
            _notificationUpvotesSwitch.CheckedChange += NotificationChange;
            _notificationCommentsUpvotesSwitch.CheckedChange += NotificationChange;
            _notificationFollowingSwitch.CheckedChange += NotificationChange;
            _notificationCommentsSwitch.CheckedChange += NotificationChange;
            _notificationPostingSwitch.CheckedChange += NotificationChange;

            //for tests
            if (AppSettings.User.IsDev || AppSettings.User.Login.Equals("joseph.kalu"))
            {
                _testsButton.Visibility = ViewStates.Visible;
                _testsButton.Click += StartTestActivity;
            }
        }

        protected override void OnResume()
        {
            _addAccountLoader.Visibility = ViewStates.Gone;
            _addButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AddAccountText);
            _addButton.Enabled = true;
            base.OnResume();
        }

        public override void OnBackPressed()
        {
            if (_nsfwChanged || _lowRatedChanged)
                BasePresenter.ProfileUpdateType = ProfileUpdateType.Full;
            base.OnBackPressed();
        }

        protected override async void OnDestroy()
        {
            base.OnDestroy();
            await SavePushSettings();
            Cheeseknife.Reset(this);
        }

        private void OnLowRatedSwitcherOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            AppSettings.User.IsLowRated = _lowRatedSwitcher.Checked;
            _lowRatedChanged = !_lowRatedChanged;
        }

        private void OnNsfwSwitcherOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            AppSettings.User.IsNsfw = _nsfwSwitcher.Checked;
            _nsfwChanged = !_nsfwChanged;
        }

        private void NotificationChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (!(sender is SwitchCompat))
                return;

            var subscription = PushSettings.None;

            if (Equals(sender, _notificationUpvotesSwitch))
                subscription = PushSettings.Upvote;
            else if (Equals(sender, _notificationCommentsUpvotesSwitch))
                subscription = PushSettings.UpvoteComment;
            else if (Equals(sender, _notificationFollowingSwitch))
                subscription = PushSettings.Follow;
            else if (Equals(sender, _notificationCommentsSwitch))
                subscription = PushSettings.Comment;
            else if (Equals(sender, _notificationPostingSwitch))
                subscription = PushSettings.User;

            if (e.IsChecked)
                PushSettings |= subscription;
            else
                PushSettings ^= subscription;
        }

        private async Task SavePushSettings()
        {
            if (AppSettings.User.PushSettings == PushSettings)
                return;

            var model = new PushNotificationsModel(AppSettings.User.UserInfo, true)
            {
                Subscriptions = PushSettings.FlagToStringList()
            };
            var resp = await BasePresenter.TrySubscribeForPushes(model);
            if (resp.IsSuccess)
                AppSettings.User.PushSettings = PushSettings;
            else
                this.ShowAlert(resp.Error);
        }

        private void OnAdapterPickAccount(UserInfo userInfo)
        {
            if (userInfo == null)
                return;

            SwitchChain(userInfo);
        }

        private void OnAdapterDeleteAccount(UserInfo userInfo)
        {
            if (userInfo == null)
                return;

            OneSignal.Current.DeleteTag("username");
            OneSignal.Current.DeleteTag("player_id");
            var chainToDelete = userInfo.Chain;
            AppSettings.User.Delete(userInfo);
            RemoveChain(chainToDelete);
            _accountsAdapter.NotifyDataSetChanged();
        }

        private void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private void TermsOfServiceClick(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse(Constants.Tos);
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        private void GuideClick(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse(Constants.Guide);
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        }

        private async void AddAccountClick(object sender, EventArgs e)
        {
            _addAccountLoader.Visibility = ViewStates.Visible;
            _addButton.Text = string.Empty;
            _addButton.Enabled = false;
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
            var accounts = AppSettings.User.GetAllAccounts();
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
