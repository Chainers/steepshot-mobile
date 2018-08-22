using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Fragment
{
    public sealed class FollowersFragment : BaseFragmentWithPresenter<UserFriendPresenter>
    {
        public const string IsFollowersExtra = "isFollowers";
        public const string UsernameExtra = "username";
        public const string CountExtra = "count";

        private FollowersAdapter _adapter;
        private string _username;
        private bool _isFollowers;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [BindView(Resource.Id.followers_list)] private RecyclerView _followersList;
        [BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.people_count)] private TextView _peopleCount;
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_followers, null);
                Cheeseknife.Bind(this, InflatedView);
            }
            ToggleTabBar();
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _isFollowers = Activity.Intent.GetBooleanExtra(IsFollowersExtra, false);
            Presenter.FollowType = _isFollowers ? FriendsType.Followers : FriendsType.Following;

            Presenter.SourceChanged += PresenterSourceChanged;

            var count = Activity.Intent.GetIntExtra(CountExtra, 0);
            _peopleCount.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PeopleText, count);
            _username = Activity.Intent.GetStringExtra(UsernameExtra) ?? AppSettings.User.Login;

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBackClick;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Presenter.FollowType.GetDescription();

            _viewTitle.Typeface = Style.Semibold;
            _peopleCount.Typeface = Style.Regular;

            _adapter = new FollowersAdapter(Activity, Presenter);
            _adapter.FollowAction += Follow;
            _adapter.UserAction += UserAction;

            var scrollListner = new ScrollListener();
            scrollListner.ScrolledToBottom += LoadItems;

            _followersList.SetAdapter(_adapter);
            _followersList.SetLayoutManager(new LinearLayoutManager(Activity));
            _followersList.AddOnScrollListener(scrollListner);

            _emptyQueryLabel.Typeface = Style.Light;
            _emptyQueryLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EmptyCategory);

            LoadItems();
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }


        public void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;
            Activity.RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
            AppSettings.ProfileUpdateType = ProfileUpdateType.OnlyInfo;
        }

        private async void Follow(UserFriend userFriend)
        {
            if (userFriend == null)
                return;
            if (AppSettings.User.HasPostingPermission)
            {
                var exception = await Presenter.TryFollow(userFriend);
                if (!IsInitialized)
                    return;

                Context.ShowAlert(exception, ToastLength.Long);
            }
            else
            {
                var intent = new Intent(Activity, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private async void LoadItems()
        {
            var exception = await Presenter.TryLoadNextUserFriends(_username);
            if (!IsInitialized)
                return;

            Context.ShowAlert(exception, ToastLength.Long);
            _bar.Visibility = ViewStates.Gone;

            _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        private void UserAction(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(userFriend.Author));
        }

        public override void OnResume()
        {
            base.OnResume();
            _adapter.NotifyDataSetChanged();
        }
    }
}
