using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.Fragments;
using Steemix.Droid.ViewModels;

namespace Steemix.Droid.Activities
{
    [Activity(Label = "GuestActivity")]
    public class GuestActivity : BaseActivity<FeedViewModel>, View.IOnScrollChangeListener
    {
        [InjectView(Resource.Id.feed_list)]
        RecyclerView FeedList;

        [InjectView(Resource.Id.loading_spinner)]
        ProgressBar Bar;

        Adapter.FeedAdapter FeedAdapter;

        [InjectView(Resource.Id.pop_up_arrow)]
        ImageView arrow;

        [InjectView(Resource.Id.btn_login)]
        Button Login;

        [InjectOnClick(Resource.Id.btn_login)]
        public void OnLoginClick(object sender, EventArgs e)
        {
            OpenLogin();
        }

        [InjectOnClick(Resource.Id.Title)]
        public void OnTitleClick(object sender, EventArgs e)
        {
            if (SupportFragmentManager.FindFragmentByTag(FollowingFragmentId) == null)
                ShowFollowing();
            else
                HideFollowing();
        }

        public const string FollowingFragmentId = "FollowingFragment";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_feed);
            Cheeseknife.Inject(this);

            FeedAdapter = new Adapter.FeedAdapter(this, ViewModel.Posts);
            FeedList.SetAdapter(FeedAdapter);
            FeedList.SetLayoutManager(new LinearLayoutManager(this));
            FeedList.SetOnScrollChangeListener(this);
            FeedAdapter.LikeAction += FeedAdapter_LikeAction;
            Login.Visibility = ViewStates.Visible;
        }

        async void FeedAdapter_LikeAction(int position)
        {
            if (UserPrincipal.IsAuthenticated)
            {
                var response = await ViewModel.Vote(ViewModel.Posts[position]);
                if (response.Success)
                {
                    ViewModel.Posts[position].Vote = !ViewModel.Posts[position].Vote;
                    FeedAdapter.NotifyDataSetChanged();
                }
                else
                {
                    //TODO:KOA Show error
                }
            }
            else
            {
                OpenLogin();
            }
        }

        private void OpenLogin()
        {
            Intent intent = new Intent(this, typeof(SignInActivity));
            StartActivity(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            FeedAdapter.NotifyDataSetChanged();
            ViewModel.Posts.CollectionChanged += Posts_CollectionChanged;
        }

        protected override void OnPause()
        {
            ViewModel.Posts.CollectionChanged -= Posts_CollectionChanged;
            base.OnPause();
        }

        void Posts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (Bar.Visibility == ViewStates.Visible)
                    Bar.Visibility = ViewStates.Gone;
                FeedAdapter.NotifyDataSetChanged();
            });
        }

        public void ShowFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(this, Resource.Animation.rotate180));
            SupportFragmentManager.BeginTransaction()
                                .SetCustomAnimations(Resource.Animation.up_down, Resource.Animation.down_up, Resource.Animation.up_down, Resource.Animation.down_up)
                                .Add(Resource.Id.fragment_container, new FollowingFragment(), FollowingFragmentId)
                                .Commit();
        }

        public void HideFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(this, Resource.Animation.rotate0));
            SupportFragmentManager.BeginTransaction()
                                  .Remove(SupportFragmentManager.FindFragmentByTag(FollowingFragmentId))
                                .Commit();
        }

        int prevPos = 0;
        public void OnScrollChange(View v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
            int pos = ((LinearLayoutManager)FeedList.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
            if (pos > prevPos && pos != prevPos)
            {
                if (pos == FeedList.GetAdapter().ItemCount - 1)
                {
                    if (pos < FeedAdapter.ItemCount)
                    {
                        Task.Run(() => ViewModel.GetTopPosts(FeedAdapter.GetItem(FeedAdapter.ItemCount - 1).Url, 10));
                        prevPos = pos;
                    }
                }
            }
        }
    }
}
