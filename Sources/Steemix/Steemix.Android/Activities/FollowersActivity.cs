using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.Adapter;
using Steemix.Droid.ViewModels;
using Sweetshot.Library.Models.Requests;

namespace Steemix.Droid.Activities
{
    [Activity]
    public class FollowersActivity : BaseActivity<FollowersViewModel>, View.IOnScrollChangeListener
    {
        private FollowType _friendsType;

        private FollowersAdapter _followersAdapter;

        [InjectView(Resource.Id.followers_list)]
        private RecyclerView _followersList;

        [InjectView(Resource.Id.loading_spinner)]
        private ProgressBar _bar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var isFollow = Intent.GetBooleanExtra("isFollow", false);
            _friendsType = isFollow ? FollowType.Follow : FollowType.UnFollow;
            ViewModel.ViewLoad(_friendsType);
            SetContentView(Resource.Layout.lyt_followers);
            Cheeseknife.Inject(this);

            _followersAdapter = new FollowersAdapter(this, ViewModel.Collection);
            _followersList.SetAdapter(_followersAdapter);
            _followersList.SetLayoutManager(new LinearLayoutManager(this));
            _followersList.SetOnScrollChangeListener(this);
            _followersAdapter.FollowAction += FollowersAdapter_FollowAction;
        }

        async void FollowersAdapter_FollowAction(int position)
        {
            var response = await ViewModel.Follow(ViewModel.Collection[position]);
            if (response.Success)
            {
                ViewModel.Collection[position].FollowUnfollow = !ViewModel.Collection[position].FollowUnfollow;
                _followersAdapter.NotifyDataSetChanged();
            }
            else
            {
                //TODO:KOA Show error
            }
        }

        [InjectOnClick(Resource.Id.go_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Finish();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _followersAdapter.NotifyDataSetChanged();
            ViewModel.Collection.CollectionChanged += CollectionChanged;
        }

        protected override void OnPause()
        {
            ViewModel.Collection.CollectionChanged -= CollectionChanged;
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
            int pos = ((LinearLayoutManager)_followersList.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
            if (pos > _prevPos && pos != _prevPos)
            {
                if (pos == _followersList.GetAdapter().ItemCount - 1)
                {
                    if (pos < _followersAdapter.ItemCount)
                    {
                        Task.Run(() => ViewModel.GetItems(_followersAdapter.GetItem(_followersAdapter.ItemCount - 1).Author, 10, _friendsType));
                        _prevPos = pos;
                    }
                }
            }
        }
    }
}