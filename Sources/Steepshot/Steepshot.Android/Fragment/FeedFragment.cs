using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class FeedFragment : BaseFragmentWithPresenter<FeedPresenter>
    {
        private FeedAdapter _feedAdapter;
        private ScrollListener _scrollListner;
        public static int SearchRequestCode = 1336;
        public const string FollowingFragmentId = nameof(DropdownFragment);
        public string CustomTag
        {
            get => _presenter.Tag;
            set => _presenter.Tag = value;
        }
        private readonly bool _isFeed;
        private Typeface font;
        private Typeface semibold_font;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.feed_list)] private RecyclerView _feedList;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.feed_refresher)] private SwipeRefreshLayout _refresher;
#pragma warning restore 0649

        public FeedFragment(bool isFeed = false)
        {
            _isFeed = isFeed;
        }
        /*
        [InjectOnClick(Resource.Id.btn_login)]
        public void OnLogin(object sender, EventArgs e)
        {
            OpenLogin();
        }*/

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
            /*
            try
            {
                var s = Activity.Intent.GetStringExtra("SEARCH");
                if (s != null && s != CustomTag && _bar != null)
                {
                    Activity.Intent.RemoveExtra("SEARCH");
                    Title.Text = _presenter.Tag = CustomTag = s;
                    _bar.Visibility = ViewStates.Visible;
                    _scrollListner.ClearPosition();
                    LoadPosts(true);
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }*/
            if (IsInitialized)
                return;
            base.OnViewCreated(view, savedInstanceState);
            //if (BasePresenter.User.IsAuthenticated)
            //_login.Visibility = ViewStates.Gone;
            font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            semibold_font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");
            //Title.Typeface = font;
            //_login.Typeface = font;
            /*if (_isFeed)
            {
                Title.Text = "Feed";
                _arrow.Visibility = ViewStates.Gone;
                _search.Visibility = ViewStates.Gone;
            }
            else
                Title.Text = "Trending";
*/
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
            if(clearOld)
                _presenter.Clear();

            List<string> errors;
            if (string.IsNullOrEmpty(CustomTag))
                errors = await _presenter.TryLoadNextTopPosts();
            else
                errors = await _presenter.TryLoadNextSearchedPosts();

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

        public void OnSearchPosts(string title, PostType type)
        {
            //Title.Text = title;
            _bar.Visibility = ViewStates.Visible;
            _presenter.PostType = type;
            _presenter.Tag = null;
            _scrollListner.ClearPosition();
            LoadPosts(true);
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
            else
                OpenLogin();
        }
        /*
        public void ShowDropdown()
        {
            _arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate180));
            ChildFragmentManager.BeginTransaction()
                                .SetCustomAnimations(Resource.Animation.up_down, Resource.Animation.down_up, Resource.Animation.up_down, Resource.Animation.down_up)
                                .Add(Resource.Id.fragment_container, new DropdownFragment(this), FollowingFragmentId)
                                .Commit();
        }

        public void HideDropdown()
        {
            _arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate0));
            ChildFragmentManager.BeginTransaction()
                                .Remove(ChildFragmentManager.FindFragmentByTag(FollowingFragmentId))
                                .Commit();
        }
*/
        protected override void CreatePresenter()
        {
            _presenter = new FeedPresenter(_isFeed);
        }

        private void OpenLogin()
        {
            var intent = new Intent(Activity, typeof(PreSignInActivity));
            StartActivity(intent);
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }
    }
}
