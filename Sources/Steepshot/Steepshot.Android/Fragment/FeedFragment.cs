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
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class FeedFragment : BaseFragmentWithPresenter<FeedPresenter>
    {
        private FeedAdapter _feedAdapter;
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

            _feedAdapter = new FeedAdapter(Context, _presenter);
            _feedList.SetAdapter(_feedAdapter);
            _feedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += () => LoadPosts();
            _feedList.AddOnScrollListener(_scrollListner);
            _feedAdapter.LikeAction += LikeAction;
            _feedAdapter.UserAction += UserAction;
            _feedAdapter.CommentAction += CommentAction;
            _feedAdapter.VotersClick += VotersAction;
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
            intent.PutExtra(PostPreviewActivity.PhotoExtraPath, photo);
            StartActivity(intent);
        }

        private void CommentAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra(CommentsActivity.PostExtraPath, post.Url);
            Context.StartActivity(intent);
        }

        private void VotersAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            Activity.Intent.PutExtra("url", post.Url);
            Activity.Intent.PutExtra("count", post.NetVotes);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private void UserAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private async void LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                _feedAdapter.ActionsEnabled = false;
                var errors = await _presenter.TryVote(position);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors);
                else
                {
                    await Task.Delay(3000);
                }
                _feedAdapter.ActionsEnabled = true;
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
