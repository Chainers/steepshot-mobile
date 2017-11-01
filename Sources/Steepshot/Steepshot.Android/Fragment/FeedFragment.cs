using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class FeedFragment : BaseFragmentWithPresenter<FeedPresenter>
    {
        public const string PostUrlExtraPath = "url";
        public const string PostNetVotesExtraPath = "count";

        private FeedAdapter _adapter;
        private ScrollListener _scrollListner;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.feed_list)] private RecyclerView _feedList;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.feed_refresher)] private SwipeRefreshLayout _refresher;
#pragma warning restore 0649


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_feed, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            Presenter.SourceChanged += PresenterSourceChanged;
            _adapter = new FeedAdapter(Context, Presenter);
            _adapter.LikeAction += LikeAction;
            _adapter.UserAction += UserAction;
            _adapter.CommentAction += CommentAction;
            _adapter.VotersClick += VotersAction;
            _adapter.PhotoClick += PhotoClick;

            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += LoadPosts;

            _refresher.Refresh += OnRefresh;

            _feedList.SetAdapter(_adapter);
            _feedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
            _feedList.AddOnScrollListener(_scrollListner);

            LoadPosts();
        }


        [InjectOnClick(Resource.Id.logo)]
        public void OnPost(object sender, EventArgs e)
        {
            _feedList.ScrollToPosition(0);
        }

        private void PresenterSourceChanged()
        {
            if (IsDetached || IsRemoving)
                return;

            Activity.RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            _scrollListner.ClearPosition();
            Presenter.Clear();
            LoadPosts();
        }

        private async void LoadPosts()
        {
            var errors = await Presenter.TryLoadNextTopPosts();
            if (IsDetached || IsRemoving)
                return;

            Context.ShowAlert(errors);

            _bar.Visibility = ViewStates.Gone;
            _refresher.Refreshing = false;
        }

        private void PhotoClick(Post post)
        {
            if (post == null)
                return;

            var photo = post.Photos?.FirstOrDefault();
            if (photo == null)
                return;

            var intent = new Intent(Context, typeof(PostPreviewActivity));
            intent.PutExtra(PostPreviewActivity.PhotoExtraPath, photo);
            StartActivity(intent);
        }

        private void CommentAction(Post post)
        {
            if (post == null)
                return;

            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra(CommentsActivity.PostExtraPath, post.Url);
            Context.StartActivity(intent);
        }

        private void VotersAction(Post post)
        {
            if (post == null)
                return;

            Activity.Intent.PutExtra(PostUrlExtraPath, post.Url);
            Activity.Intent.PutExtra(PostNetVotesExtraPath, post.NetVotes);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private void UserAction(Post post)
        {
            if (post == null)
                return;

            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private async void LikeAction(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
                return;

            var errors = await Presenter.TryVote(post);
            if (IsDetached || IsRemoving)
                return;

            Context.ShowAlert(errors);
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }
    }
}
