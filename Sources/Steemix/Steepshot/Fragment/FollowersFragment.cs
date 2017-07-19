using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Sweetshot.Library.Models.Requests;
using System.Threading.Tasks;
using Android.Content;

namespace Steepshot
{
    public class FollowersFragment : BaseFragment, FollowersView
    {
		FollowersPresenter presenter;
        private FollowType _friendsType;
        private FollowersAdapter _followersAdapter;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.Title)] TextView ViewTitle;
		[InjectView(Resource.Id.followers_list)] RecyclerView _followersList;
#pragma warning restore 0649

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!_isInitialized)
			{
				v = inflater.Inflate(Resource.Layout.lyt_followers, null);
				Cheeseknife.Inject(this, v);
			}
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			if (_isInitialized)
				return;
			base.OnViewCreated(view, savedInstanceState);
			var isFollowers = Activity.Intent.GetBooleanExtra("isFollowers", false);
			var username = Activity.Intent.GetStringExtra("username") ?? User.Login;
			_friendsType = isFollowers ? FollowType.Follow : FollowType.UnFollow;

			presenter.Collection.Clear();
			presenter.ViewLoad(_friendsType, username);

			ViewTitle.Text = isFollowers ? GetString(Resource.String.text_followers) : GetString(Resource.String.text_following);

			_followersAdapter = new FollowersAdapter(Activity, presenter.Collection);
			_followersList.SetAdapter(_followersAdapter);
			_followersList.SetLayoutManager(new LinearLayoutManager(Activity));
			_followersList.AddOnScrollListener(new FollowersScrollListener(presenter, username, _friendsType));
			_followersAdapter.FollowAction += FollowersAdapter_FollowAction;
			_followersAdapter.UserAction += FollowersAdapter_UserAction;
		}

        public class FollowersScrollListener : RecyclerView.OnScrollListener
        {
			FollowersPresenter presenter;
			private string _username;
			private FollowType _followType;

			public FollowersScrollListener(FollowersPresenter presenter, string username, FollowType followType)
			{
				this.presenter = presenter;
				_username = username;
				_followType = followType;
			}
			int prevPos = 0;
			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
				if (pos > prevPos && pos != prevPos)
				{
					if (pos == recyclerView.GetAdapter().ItemCount - 1)
					{
						if (pos < ((FollowersAdapter)recyclerView.GetAdapter()).ItemCount)
						{
							Task.Run(() => presenter.GetItems(_followType, _username));
							prevPos = pos;
						}
					}
				}
			}

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {

            }
        }

        async void FollowersAdapter_FollowAction(int position)
        {
			try
			{
				var response = await presenter.Follow(presenter.Collection[position]);
				if (response.Success)
				{
					presenter.Collection[position].IsFollow = !presenter.Collection[position].IsFollow;
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
				Reporter.SendCrash(ex);
			}
        }

		private void FollowersAdapter_UserAction(int position)
		{
			((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(presenter.Collection[position].Author));
		}

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

		public override void OnResume()
		{
			base.OnResume();
			_followersAdapter.NotifyDataSetChanged();
            presenter.Collection.CollectionChanged += CollectionChanged;
		}

		public override void OnPause()
		{
			base.OnPause();
			_followersAdapter.NotifyDataSetChanged();
            presenter.Collection.CollectionChanged += CollectionChanged;
		}

        void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Activity.RunOnUiThread(() =>
            {
                if (_bar.Visibility == ViewStates.Visible)
                    _bar.Visibility = ViewStates.Gone;
                _followersAdapter.NotifyDataSetChanged();
            });
        }

		protected override void CreatePresenter()
		{
			presenter = new FollowersPresenter(this);
		}

		public override void OnDetach()
		{
			base.OnDetach();
			Cheeseknife.Reset(this);
		}
	}
}