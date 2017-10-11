using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class FeedFragment : BaseFragmentWithPresenter<FeedPresenter>
    {
        private FeedAdapter _feedAdapter;
        private ScrollListener _scrollListner;
        private Typeface font;
        private Typeface semibold_font;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.feed_list)] private RecyclerView _feedList;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.feed_refresher)] private SwipeRefreshLayout _refresher;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                V = inflater.Inflate(Resource.Layout.lyt_feed, null);
                Cheeseknife.Inject(this, V);
            }
            return V;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;
            base.OnViewCreated(view, savedInstanceState);

            font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            semibold_font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");

            _feedAdapter = new FeedAdapter(Context, _presenter, new[] { font, semibold_font });
            _feedList.SetAdapter(_feedAdapter);
            _feedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += () => LoadPosts();
            _feedList.AddOnScrollListener(_scrollListner);
            _feedAdapter.LikeAction += FeedAdapter_LikeAction;
            _feedAdapter.UserAction += FeedAdapter_UserAction;
            _feedAdapter.CommentAction += FeedAdapter_CommentAction;
            _feedAdapter.VotersClick += FeedAdapter_VotersAction;
            _feedAdapter.PhotoClick += PhotoClick;
            LoadPosts();
            _refresher.Refresh += delegate
                {
                    _scrollListner.ClearPosition();
                    LoadPosts(true);
                };
        }

        private async void LoadPosts(bool clearOld = false)
        {
            if (clearOld)
                _presenter.Clear();

            var errors = await _presenter.TryLoadNextTopPosts();
            if (_bar != null)
            {
                _bar.Visibility = ViewStates.Gone;
                _refresher.Refreshing = false;
            }

            if (errors == null)
                return;

            ShowAlert(errors);
            _feedAdapter?.NotifyDataSetChanged();
        }

        public void PhotoClick(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            var photo = post.Photos?.FirstOrDefault();
            if (photo == null)
                return;
            var intent = new Intent(Context, typeof(PostPreviewActivity));
            intent.PutExtra("PhotoURL", photo);
            StartActivity(intent);
        }

        void FeedAdapter_CommentAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra("uid", post.Url);
            Context.StartActivity(intent);
        }

        void FeedAdapter_VotersAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            Activity.Intent.PutExtra("url", post.Url);
            Activity.Intent.PutExtra("count", post.NetVotes);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        void FeedAdapter_UserAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private async void FeedAdapter_LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await _presenter.TryVote(position);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors);

                _feedAdapter?.NotifyDataSetChanged();
            }
        }

        protected override void CreatePresenter()
        {
            _presenter = new FeedPresenter(true);
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }
    }
}
