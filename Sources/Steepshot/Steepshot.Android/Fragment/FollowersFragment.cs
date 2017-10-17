using System;
using System.Linq;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class FollowersFragment : BaseFragmentWithPresenter<FollowersPresenter>
    {
        private FollowersAdapter<UserFriend> _followersAdapter;
        private string _username;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.btn_back)] ImageButton _backButton;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.followers_list)] private RecyclerView _followersList;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.people_count)] private TextView _people_count;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                V = inflater.Inflate(Resource.Layout.lyt_followers, null);
                Cheeseknife.Inject(this, V);
            }
            return V;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;
            base.OnViewCreated(view, savedInstanceState);

            var font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            var semibold_font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");


            var count = Activity.Intent.GetIntExtra("count", 0);
            _people_count.Text = $"{count.ToString("N0")} people";
            _username = Activity.Intent.GetStringExtra("username") ?? BasePresenter.User.Login;

            LoadItems();

            _backButton.Visibility = ViewStates.Visible;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Text = _presenter.FollowType.GetDescription();

            _viewTitle.Typeface = semibold_font;
            _people_count.Typeface = font;

            _followersAdapter = new FollowersAdapter<UserFriend>(Activity, _presenter, new[] { font, semibold_font });
            _followersList.SetAdapter(_followersAdapter);
            _followersList.SetLayoutManager(new LinearLayoutManager(Activity));
            var scrollListner = new ScrollListener();
            scrollListner.ScrolledToBottom += LoadItems;
            _followersList.AddOnScrollListener(scrollListner);
            _followersAdapter.FollowAction += Follow;
            _followersAdapter.UserAction += UserAction;
        }

        private async void Follow(int position)
        {
            var errors = await _presenter.TryFollow(_presenter[position]);
            if (errors == null)
                return;

            if (errors.Any())
                ShowAlert(errors, ToastLength.Short);
            else
            {
                _followersAdapter.NotifyDataSetChanged();
            }
        }

        private void LoadItems()
        {
            _presenter.TryLoadNextUserFriends(_username).ContinueWith((e) =>
            {
                var errors = e.Result;
                Activity.RunOnUiThread(() =>
                {
                    if (_bar != null)
                        _bar.Visibility = ViewStates.Gone;
                    if (errors != null && errors.Count > 0)
                        ShowAlert(errors, ToastLength.Long);
                    else
                        _followersAdapter?.NotifyDataSetChanged();
                });
            });
        }

        private void UserAction(int position)
        {
            var user = _presenter[position];
            if (user == null)
                return;

            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(user.Author));
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        protected override void CreatePresenter()
        {
            var isFollowers = Activity.Intent.GetBooleanExtra("isFollowers", false);
            _presenter = new FollowersPresenter(isFollowers ? FriendsType.Followers : FriendsType.Following);
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }
    }
}
