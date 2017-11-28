using System;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;

namespace Steepshot.Fragment
{
    public sealed class FeedFragment : BaseFragmentWithPresenter<FeedPresenter>
    {
        public const string PostUrlExtraPath = "url";
        public const string PostNetVotesExtraPath = "count";

        private FeedAdapter<FeedPresenter> _adapter;
        private ScrollListener _scrollListner;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.feed_list)] private RecyclerView _feedList;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.feed_refresher)] private SwipeRefreshLayout _refresher;
        [InjectView(Resource.Id.logo)] private ImageView _logo;
        [InjectView(Resource.Id.app_bar)] private AppBarLayout _toolbar;
        [InjectView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
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

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == -1 && requestCode == CommentsActivity.RequestCode)
            {
                var postUrl = data.GetStringExtra(CommentsActivity.ResultString);
                var count = data.GetIntExtra(CommentsActivity.CountString, 0);
                var post = Presenter.FirstOrDefault(p => p.Url == postUrl);
                post.Children += count;
                _adapter.NotifyDataSetChanged();
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            Presenter.SourceChanged += PresenterSourceChanged;
            _adapter = new FeedAdapter<FeedPresenter>(Context, Presenter);
            _adapter.LikeAction += LikeAction;
            _adapter.UserAction += UserAction;
            _adapter.CommentAction += CommentAction;
            _adapter.VotersClick += VotersAction;
            _adapter.PhotoClick += PhotoClick;
            _adapter.FlagAction += FlagAction;
            _adapter.HideAction += HideAction;
            _adapter.TagAction += TagAction;
            _logo.Click += OnLogoClick;
            _toolbar.OffsetChanged += OnToolbarOffsetChanged;

            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += LoadPosts;

            _refresher.Refresh += OnRefresh;

            _feedList.SetAdapter(_adapter);
            _feedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
            _feedList.AddOnScrollListener(_scrollListner);


            _emptyQueryLabel.Typeface = Style.Light;
            _emptyQueryLabel.Text = Localization.Texts.EmptyQuery;

            LoadPosts();
        }

        private void OnToolbarOffsetChanged(object sender, AppBarLayout.OffsetChangedEventArgs e)
        {
            ViewCompat.SetElevation(_toolbar, BitmapUtils.DpToPixel(2, Resources));
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        private void OnLogoClick(object sender, EventArgs e)
        {
            _feedList.ScrollToPosition(0);
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
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
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors);

            _bar.Visibility = ViewStates.Gone;
            _refresher.Refreshing = false;

            _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
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
            StartActivityForResult(intent, CommentsActivity.RequestCode);
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
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors);
        }

        private async void FlagAction(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
                return;

            var errors = await Presenter.TryFlag(post);
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors);
        }

        private void HideAction(Post post)
        {
            Presenter.RemovePost(post);
        }

        private void TagAction(string tag)
        {
            if (tag != null)
            {
                Activity.Intent.PutExtra(SearchFragment.SearchExtra, tag);
                ((RootActivity)Activity).SelectTabWithClearing(1);
            }
            else
                _adapter.NotifyDataSetChanged();
        }
    }
}
