using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;

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
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.followers_list)] private RecyclerView _followersList;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.people_count)] private TextView _peopleCount;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_followers, null);
                Cheeseknife.Inject(this, InflatedView);
            }
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
            _peopleCount.Text = $"{count:N0} {Localization.Texts.PeopleText}";
            _username = Activity.Intent.GetStringExtra(UsernameExtra) ?? BasePresenter.User.Login;

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
            _emptyQueryLabel.Text = Localization.Texts.EmptyQuery;

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
            if (!_isFollowers && _username == BasePresenter.User.Login)
                _peopleCount.Text = $"{Presenter.FindAll(u => u.HasFollowed).Count:N0} {Localization.Texts.PeopleText}";
            Activity.RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
            BasePresenter.ShouldUpdateProfile = true;
        }

        private async void Follow(UserFriend userFriend)
        {
            if (userFriend == null)
                return;
            var errors = await Presenter.TryFollow(userFriend);
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors, ToastLength.Long);
        }

        private async void LoadItems()
        {
            var errors = await Presenter.TryLoadNextUserFriends(_username);
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors, ToastLength.Long);
            _bar.Visibility = ViewStates.Gone;

            _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        private void UserAction(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(userFriend.Author));
        }
    }
}
