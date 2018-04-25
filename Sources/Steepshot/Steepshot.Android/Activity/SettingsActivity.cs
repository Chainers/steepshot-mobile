using System;
using System.Collections.Generic;
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
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class SettingsActivity : BaseActivityWithPresenter<UserProfilePresenter>
    {
        private AccountsAdapter _accountsAdapter;
        private bool _lowRatedChanged;
        private bool _nsfwChanged;
        private List<PushSubscription> _pushSubscriptions;

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
            var accounts = BasePresenter.User.GetAllAccounts();

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

            _nsfwSwitcher.Checked = BasePresenter.User.IsNsfw;
            _lowRatedSwitcher.Checked = BasePresenter.User.IsLowRated;
            _notificationUpvotesSwitch.Checked = BasePresenter.User.PushSubscriptions.Contains(PushSubscription.Upvote);
            _notificationCommentsUpvotesSwitch.Checked = BasePresenter.User.PushSubscriptions.Contains(PushSubscription.UpvoteComment);
            _notificationFollowingSwitch.Checked = BasePresenter.User.PushSubscriptions.Contains(PushSubscription.Follow);
            _notificationCommentsSwitch.Checked = BasePresenter.User.PushSubscriptions.Contains(PushSubscription.Comment);
            _notificationPostingSwitch.Checked = BasePresenter.User.PushSubscriptions.Contains(PushSubscription.User);

            _nsfwSwitcher.CheckedChange += OnNsfwSwitcherOnCheckedChange;
            _lowRatedSwitcher.CheckedChange += OnLowRatedSwitcherOnCheckedChange;
            _notificationUpvotesSwitch.CheckedChange += NotificationUpvotesSwitchOnCheckedChange;
            _notificationCommentsUpvotesSwitch.CheckedChange += NotificationCommentsUpvotesSwitchOnCheckedChange;
            _notificationFollowingSwitch.CheckedChange += NotificationFollowingSwitchOnCheckedChange;
            _notificationCommentsSwitch.CheckedChange += NotificationCommentsSwitchOnCheckedChange;
            _notificationPostingSwitch.CheckedChange += NotificationPostingSwitchOnCheckedChange;

            _pushSubscriptions = new List<PushSubscription>();
            _pushSubscriptions.AddRange(BasePresenter.User.PushSubscriptions);
            //for tests
            if (BasePresenter.User.IsDev || BasePresenter.User.Login.Equals("joseph.kalu"))
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

        public override async void OnBackPressed()
        {
            if (_nsfwChanged || _lowRatedChanged)
                BasePresenter.ProfileUpdateType = ProfileUpdateType.Full;
            base.OnBackPressed();
            if (!BasePresenter.User.PushSubscriptions.SequenceEqual(_pushSubscriptions))
            {
                var model = new PushNotificationsModel(BasePresenter.User.UserInfo, true)
                {
                    Subscriptions = _pushSubscriptions.FindAll(x => x != PushSubscription.User).ToList()
                };
                var error = await BasePresenter.TrySubscribeForPushes(model);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        private void OnLowRatedSwitcherOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            BasePresenter.User.IsLowRated = _lowRatedSwitcher.Checked;
            _lowRatedChanged = !_lowRatedChanged;
        }

        private void OnNsfwSwitcherOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            BasePresenter.User.IsNsfw = _nsfwSwitcher.Checked;
            _nsfwChanged = !_nsfwChanged;
        }

        private void NotificationUpvotesSwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e) =>
            SwitchSubscription(PushSubscription.Upvote, e.IsChecked);

        private void NotificationCommentsUpvotesSwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e) =>
            SwitchSubscription(PushSubscription.UpvoteComment, e.IsChecked);

        private void NotificationCommentsSwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e) =>
            SwitchSubscription(PushSubscription.Comment, e.IsChecked);

        private void NotificationFollowingSwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e) =>
            SwitchSubscription(PushSubscription.Follow, e.IsChecked);

        private void NotificationPostingSwitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e) =>
            SwitchSubscription(PushSubscription.User, e.IsChecked);

        private void SwitchSubscription(PushSubscription subscription, bool value)
        {
            if (value && !_pushSubscriptions.Contains(subscription))
                _pushSubscriptions.Add(subscription);
            else
                _pushSubscriptions.Remove(subscription);
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
            BasePresenter.User.Delete(userInfo);
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
