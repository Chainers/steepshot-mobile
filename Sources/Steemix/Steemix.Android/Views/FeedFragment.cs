using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.ViewModels;
using Steemix.Droid.Views;

namespace Steemix.Droid
{
    public class FeedFragment : BaseFragment<FeedViewModel>, View.IOnScrollChangeListener
    {
        [InjectView(Resource.Id.feed_list)]
        RecyclerView FeedList;

        [InjectView(Resource.Id.loading_spinner)]
        ProgressBar Bar;

        Adapter.FeedAdapter FeedAdapter;

        [InjectView(Resource.Id.pop_up_arrow)]
        ImageView arrow;

        [InjectOnClick(Resource.Id.Title)]
        public void OnTitleClick(object sender, EventArgs e)
        {
            if (ChildFragmentManager.FindFragmentByTag(FollowingFragmentId) == null)
                ShowFollowing();
            else
                HideFollowing();
        }

        public const string FollowingFragmentId = "FollowingFragment";

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.lyt_feed, null);
            Cheeseknife.Inject(this, v);
            return v;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            FeedAdapter = new Adapter.FeedAdapter(Context, ViewModel.Posts);
            FeedList.SetAdapter(FeedAdapter);
            FeedList.SetLayoutManager(new LinearLayoutManager(Context));
            FeedList.SetOnScrollChangeListener(this);
            FeedAdapter.LikeAction += FeedAdapter_LikeAction;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            Cheeseknife.Reset(this);
        }

        async void FeedAdapter_LikeAction(int position)
        {
            var response = await ViewModel.Vote(ViewModel.Posts[position]);

            if (!response.Success)
            {
                ViewModel.Posts[position].Vote = !ViewModel.Posts[position].Vote;
                FeedAdapter.NotifyDataSetChanged();
            }
            else
            {
                Intent intent = new Intent(Context, typeof(SignInActivity));
                StartActivity(intent);
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            FeedAdapter.NotifyDataSetChanged();
            ViewModel.Posts.CollectionChanged += Posts_CollectionChanged;
        }

        public override void OnPause()
        {
            ViewModel.Posts.CollectionChanged -= Posts_CollectionChanged;
            base.OnPause();
        }

        void Posts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ((RootActivity)Context).RunOnUiThread(() =>
            {
                if (Bar.Visibility == ViewStates.Visible)
                    Bar.Visibility = ViewStates.Gone;
                FeedAdapter.NotifyDataSetChanged();
            });
        }

        public void ShowFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate180));
            ChildFragmentManager.BeginTransaction()
                                .SetCustomAnimations(Resource.Animation.up_down, Resource.Animation.down_up, Resource.Animation.up_down, Resource.Animation.down_up)
                                .Add(Resource.Id.fragment_container, new FollowingFragment(), FollowingFragmentId)
                                .Commit();
        }

        public void HideFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate0));
            ChildFragmentManager.BeginTransaction()
                                .Remove(ChildFragmentManager.FindFragmentByTag(FollowingFragmentId))
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
