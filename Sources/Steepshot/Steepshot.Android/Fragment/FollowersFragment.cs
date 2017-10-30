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
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class FollowersFragment : BaseFragmentWithPresenter<UserFriendPresenter>
    {
        private FollowersAdapter _followersAdapter;
        private string _username;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.followers_list)] private RecyclerView _followersList;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.people_count)] private TextView _peopleCount;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
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

            var isFollowers = Activity.Intent.GetBooleanExtra("isFollowers", false);
            Presenter.FollowType = isFollowers ? FriendsType.Followers : FriendsType.Following;

            var count = Activity.Intent.GetIntExtra("count", 0);
            _peopleCount.Text = $"{count:N0} {Localization.Texts.PeopleText}";
            _username = Activity.Intent.GetStringExtra("username") ?? BasePresenter.User.Login;

            _backButton.Visibility = ViewStates.Visible;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = Presenter.FollowType.GetDescription();

            _viewTitle.Typeface = Style.Semibold;
            _peopleCount.Typeface = Style.Regular;

            _followersAdapter = new FollowersAdapter(Activity, Presenter);
            _followersAdapter.FollowAction += Follow;
            _followersAdapter.UserAction += UserAction;

            var scrollListner = new ScrollListener();
            scrollListner.ScrolledToBottom += LoadItems;

            _followersList.SetAdapter(_followersAdapter);
            _followersList.SetLayoutManager(new LinearLayoutManager(Activity));
            _followersList.AddOnScrollListener(scrollListner);

            LoadItems();
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }


        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }


        private async void Follow(int position)
        {
            var errors = await Presenter.TryFollow(Presenter[position]);
            if (IsDetached || IsRemoving)
                return;

            if (errors != null && errors.Count > 0)
                Context.ShowAlert(errors, ToastLength.Long);
            else
                _followersAdapter.NotifyDataSetChanged();
        }

        private async void LoadItems()
        {
            var errors = await Presenter.TryLoadNextUserFriends(_username);
            if (IsDetached || IsRemoving)
                return;

            if (errors != null && errors.Count > 0)
                Context.ShowAlert(errors, ToastLength.Long);
            else
                _followersAdapter.NotifyDataSetChanged();

            _bar.Visibility = ViewStates.Gone;
        }

        private void UserAction(int position)
        {
            var user = Presenter[position];
            if (user == null)
                return;

            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(user.Author));
        }
    }
}
