using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class FollowersFragment : BaseFragment
    {
		private FollowersPresenter _presenter;
        private FriendsType _friendsType;
        private FollowersAdapter _followersAdapter;
        private string _username;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.Title)] private TextView _viewTitle;
		[InjectView(Resource.Id.followers_list)] private RecyclerView _followersList;
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
			var isFollowers = Activity.Intent.GetBooleanExtra("isFollowers", false);
			_username = Activity.Intent.GetStringExtra("username") ?? BasePresenter.User.Login;
            _friendsType = isFollowers ? FriendsType.Followers : FriendsType.Following;

            LoadItems();

			_viewTitle.Text = isFollowers ? GetString(Resource.String.text_followers) : GetString(Resource.String.text_following);

            _followersAdapter = new FollowersAdapter(Activity, _presenter.Users);
			_followersList.SetAdapter(_followersAdapter);
			_followersList.SetLayoutManager(new LinearLayoutManager(Activity));
            var scrollListner = new ScrollListener();
            scrollListner.ScrolledToBottom += LoadItems;
            _followersList.AddOnScrollListener(scrollListner);
			_followersAdapter.FollowAction += Follow;
			_followersAdapter.UserAction += UserAction;
		}

        async void Follow(int position)
        {
			try
			{
                var response = await _presenter.Follow(_presenter.Users[position]);
				if (response.Success)
				{
                    _presenter.Users[position].HasFollowed = !_presenter.Users[position].HasFollowed;
					_followersAdapter.NotifyDataSetChanged();
				}
				else
				{
					Toast.MakeText(Activity, response.Errors[0], ToastLength.Short).Show();
					_followersAdapter.InverseFollow(position);
					_followersAdapter.NotifyDataSetChanged();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
			}
        }

        private void LoadItems()
        {
			_presenter.GetItems(_friendsType, _username).ContinueWith((e) =>
			{
                var errors = e.Result;
				Activity.RunOnUiThread(() =>
				{
					if (_bar != null)
						_bar.Visibility = ViewStates.Gone;
                    if (errors != null && errors.Count > 0)
                       Toast.MakeText(Context, errors[0], ToastLength.Long).Show();
                    
                    else
						_followersAdapter?.NotifyDataSetChanged();
				});
			});
        }

        private void UserAction(int position)
		{
            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_presenter.Users[position].Author));
		}

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

		protected override void CreatePresenter()
		{
			_presenter = new FollowersPresenter();
		}

		public override void OnDetach()
		{
			base.OnDetach();
			Cheeseknife.Reset(this);
		}
	}
}
