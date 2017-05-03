using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Sweetshot.Library.Models.Requests;
using Android.Support.V4.Content;

namespace Steepshot
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class FollowersActivity : BaseActivity, FollowersView
    {
		FollowersPresenter presenter;

        private FollowType _friendsType;

        private FollowersAdapter _followersAdapter;
        
        private RecyclerView _followersList;

        [InjectView(Resource.Id.loading_spinner)]
        private ProgressBar _bar;

        [InjectView(Resource.Id.Title)]
        TextView ViewTitle;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var isFollowers = Intent.GetBooleanExtra("isFollowers", false);
			var username = Intent.GetStringExtra("username") ?? UserPrincipal.Instance.CurrentUser.Login ;
            _friendsType = isFollowers ? FollowType.Follow : FollowType.UnFollow;

            presenter.Collection.Clear();
			presenter.ViewLoad(_friendsType, username);
            SetContentView(Resource.Layout.lyt_followers);
            Cheeseknife.Inject(this);

            ViewTitle.Text = isFollowers ? GetString(Resource.String.text_followers) : GetString(Resource.String.text_following);

            _followersAdapter = new FollowersAdapter(this, presenter.Collection);
            _followersList = FindViewById<RecyclerView>(Resource.Id.followers_list);
            _followersList.SetAdapter(_followersAdapter);
            _followersList.SetLayoutManager(new LinearLayoutManager(this));
            _followersList.AddOnScrollListener(new FollowersScrollListener());
            _followersAdapter.FollowAction += FollowersAdapter_FollowAction;
        }

        public class FollowersScrollListener : RecyclerView.OnScrollListener
        {
            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {

            }

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {

            }
        }

        async void FollowersAdapter_FollowAction(int position)
        {
			var response = await presenter.Follow(presenter.Collection[position]);
            if (response.Success)
            {
				presenter.Collection[position].IsFollow = !presenter.Collection[position].IsFollow;
                _followersAdapter.NotifyDataSetChanged();
            }
            else
            {
                Toast.MakeText(this, response.Errors[0], ToastLength.Short).Show();
                _followersAdapter.InverseFollow(position);
                _followersAdapter.NotifyDataSetChanged();
            }
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _followersAdapter.NotifyDataSetChanged();
            presenter.Collection.CollectionChanged += CollectionChanged;
        }

        protected override void OnPause()
        {
            presenter.Collection.CollectionChanged -= CollectionChanged;
            base.OnPause();
        }

        void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (_bar.Visibility == ViewStates.Visible)
                    _bar.Visibility = ViewStates.Gone;
                _followersAdapter.NotifyDataSetChanged();
            });
        }

        int _prevPos;
        public void OnScrollChange(View v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
            //int pos = ((LinearLayoutManager)_followersList.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
            //if (pos > _prevPos && pos != _prevPos)
            //{
            //    if (pos == _followersList.GetAdapter().ItemCount - 1)
            //    {
            //        if (pos < _followersAdapter.ItemCount)
            //        {
            //            Task.Run(() => ViewModel.GetItems(_followersAdapter.GetItem(_followersAdapter.ItemCount - 1).Author, 10, _friendsType));
            //            _prevPos = pos;
            //        }
            //    }
            //}
        }

		protected override void CreatePresenter()
		{
			presenter = new FollowersPresenter(this);
		}
	}
}