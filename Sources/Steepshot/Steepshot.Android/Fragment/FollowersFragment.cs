using System;
using System.Threading.Tasks;
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

namespace Steepshot.Fragment
{
    public class FollowersFragment : BaseFragment
    {
		FollowersPresenter _presenter;
        private FollowType _friendsType;
        private FollowersAdapter _followersAdapter;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.Title)] TextView _viewTitle;
		[InjectView(Resource.Id.followers_list)] RecyclerView _followersList;
#pragma warning restore 0649

		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!IsInitialized)
			{
				V = inflater.Inflate(Resource.Layout.lyt_followers, null);
				Cheeseknife.Inject(this, V);
			}
			return V;
		}

		public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
		{
			if (IsInitialized)
				return;
			base.OnViewCreated(view, savedInstanceState);
			var isFollowers = Activity.Intent.GetBooleanExtra("isFollowers", false);
			var username = Activity.Intent.GetStringExtra("username") ?? BasePresenter.User.Login;
			_friendsType = isFollowers ? FollowType.Follow : FollowType.UnFollow;

			_presenter.Collection.Clear();
			_presenter.ViewLoad(_friendsType, username);

			_viewTitle.Text = isFollowers ? GetString(Resource.String.text_followers) : GetString(Resource.String.text_following);

			_followersAdapter = new FollowersAdapter(Activity, _presenter.Collection);
			_followersList.SetAdapter(_followersAdapter);
			_followersList.SetLayoutManager(new LinearLayoutManager(Activity));
			_followersList.AddOnScrollListener(new FollowersScrollListener(_presenter, username, _friendsType));
			_followersAdapter.FollowAction += FollowersAdapter_FollowAction;
			_followersAdapter.UserAction += FollowersAdapter_UserAction;
		}

        public class FollowersScrollListener : RecyclerView.OnScrollListener
        {
			FollowersPresenter _presenter;
			private string _username;
			private FollowType _followType;

			public FollowersScrollListener(FollowersPresenter presenter, string username, FollowType followType)
			{
				_presenter = presenter;
				_username = username;
				_followType = followType;
			}
			int _prevPos;
			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
				if (pos > _prevPos && pos != _prevPos)
				{
					if (pos == recyclerView.GetAdapter().ItemCount - 1)
					{
						if (pos < ((FollowersAdapter)recyclerView.GetAdapter()).ItemCount)
						{
							Task.Run(() => _presenter.GetItems(_followType, _username));
							_prevPos = pos;
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
				var response = await _presenter.Follow(_presenter.Collection[position]);
				if (response.Success)
				{
					_presenter.Collection[position].IsFollow = !_presenter.Collection[position].IsFollow;
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

		private void FollowersAdapter_UserAction(int position)
		{
			((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_presenter.Collection[position].Author));
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
            _presenter.Collection.CollectionChanged += CollectionChanged;
		}

		public override void OnPause()
		{
			base.OnPause();
			_followersAdapter.NotifyDataSetChanged();
            _presenter.Collection.CollectionChanged += CollectionChanged;
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
			_presenter = new FollowersPresenter();
		}

		public override void OnDetach()
		{
			base.OnDetach();
			Cheeseknife.Reset(this);
		}
	}
}