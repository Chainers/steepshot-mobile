using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Content.PM;
using System.Threading.Tasks;
using Android.Views.Animations;

namespace Steemix.Android.Activity
{
    [Activity(Label = "SteepShot", MainLauncher=true,Icon = "@mipmap/ic_launcher",ScreenOrientation = ScreenOrientation.Portrait)]
	public class FeedActivity : BaseActivity<FeedViewModel>, View.IOnScrollChangeListener
	{
        RecyclerView FeedList;
        ProgressBar Bar;
        Adapter.FeedAdapter FeedAdapter;
		ImageView arrow;
		public const string FollowingFragmentId = "FollowingFragment";

		protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_feed);

            FeedList = FindViewById<RecyclerView>(Resource.Id.feed_list);
            Bar = FindViewById<ProgressBar>(Resource.Id.loading_spinner);
            FeedList.SetLayoutManager(new LinearLayoutManager(this));
            var follow = FindViewById<TextView>(Resource.Id.Title);
            follow.Clickable = true;
			arrow = FindViewById<ImageView>(Resource.Id.pop_up_arrow);

			follow.Click += Follow_Click;

			FeedAdapter = new Adapter.FeedAdapter(this, ViewModel.Posts);
            FeedList.SetAdapter(FeedAdapter);
			FeedList.SetOnScrollChangeListener(this);

			FeedAdapter.LikeAction += FeedAdapter_LikeAction;
        }

		async void FeedAdapter_LikeAction(int position)
		{
			var response = await ViewModel.Vote(ViewModel.Posts[position]);
			if (response != null)
			{
				ViewModel.Posts[position].Vote = (response.IsVoted) ? 1 : 0;
				FeedAdapter.NotifyDataSetChanged();
			}
			else
			{
				StartActivity(typeof(SignInActivity));
			}
		}

		void Follow_Click(object sender, System.EventArgs e)
		{
			if (SupportFragmentManager.FindFragmentByTag(FollowingFragmentId) == null)
				ShowFollowing();
			else
				HideFollowing();
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

        int prevPos=0;
        public void OnScrollChange(View v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
           int pos = ((LinearLayoutManager)FeedList.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
            if (pos > prevPos && pos != prevPos)
            {
                if (pos == FeedList.GetAdapter().ItemCount - 1)
                {
                    if (pos < FeedAdapter.ItemCount)
                    {
						Task.Run(() =>ViewModel.GetTopPosts(FeedAdapter.GetItem(FeedAdapter.ItemCount - 1).Url, 10));
                        prevPos = pos;
                    }
                }
            }
        }
    }
}