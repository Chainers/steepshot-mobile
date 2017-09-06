using System;
using System.Collections.Generic;
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
        [InjectView(Resource.Id.feed_list)] RecyclerView _feedList;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar _bar;
        [InjectView(Resource.Id.pop_up_arrow)] ImageView _arrow;
        [InjectView(Resource.Id.btn_login)] Button _login;
        [InjectView(Resource.Id.Title)] public TextView Title;
        [InjectView(Resource.Id.feed_refresher)] SwipeRefreshLayout _refresher;
        [InjectView(Resource.Id.btn_search)] ImageButton _search;
#pragma warning restore 0649

        public FeedFragment(bool isFeed = false)
        {
            _isFeed = isFeed;
        }

        [InjectOnClick(Resource.Id.btn_search)]
        public void OnSearch(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new SearchFragment());
        }

        [InjectOnClick(Resource.Id.btn_login)]
        public void OnLogin(object sender, EventArgs e)
        {
            OpenLogin();
        }

        [InjectOnClick(Resource.Id.Title)]
        public void OnTitleClick(object sender, EventArgs e)
        {
            if (_isFeed)
                return;
            if (ChildFragmentManager.FindFragmentByTag(FollowingFragmentId) == null)
                ShowDropdown();
            else
                HideDropdown();
        }

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
            try
            {
                var s = Activity.Intent.GetStringExtra("SEARCH");
                if (s != null && s != CustomTag && _bar != null)
                {
                    Activity.Intent.RemoveExtra("SEARCH");
                    Title.Text = _presenter.Tag = CustomTag = s;
                    _bar.Visibility = ViewStates.Visible;
                    _presenter.ClearPosts();
                    _scrollListner.ClearPosition();
                    LoadPosts();
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
            }
            if (IsInitialized)
                return;
            base.OnViewCreated(view, savedInstanceState);
            if (BasePresenter.User.IsAuthenticated)
                _login.Visibility = ViewStates.Gone;
            font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            semibold_font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");
            Title.Typeface = font;
            _login.Typeface = font;
            if (_isFeed)
            {
                Title.Text = "Feed";
                _arrow.Visibility = ViewStates.Gone;
                _search.Visibility = ViewStates.Gone;
            }
            else
                Title.Text = "Trending";

            _feedAdapter = new FeedAdapter(Context, _presenter.Posts, new Typeface[] { font, semibold_font });
            _feedList.SetAdapter(_feedAdapter);
            _feedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += LoadPosts;
            _feedList.AddOnScrollListener(_scrollListner);
            _feedAdapter.LikeAction += FeedAdapter_LikeAction;
            _feedAdapter.UserAction += FeedAdapter_UserAction;
            _feedAdapter.CommentAction += FeedAdapter_CommentAction;
            _feedAdapter.VotersClick += FeedAdapter_VotersAction;
            _feedAdapter.PhotoClick += PhotoClick;
            LoadPosts();
            _refresher.Refresh += delegate
                {
                    _presenter.ClearPosts();
                    _scrollListner.ClearPosition();
                    LoadPosts();
                };
        }

        private async void LoadPosts()
        {
            List<string> errors;
            if (string.IsNullOrEmpty(CustomTag))
                errors = await _presenter.GetTopPosts();
            else
                errors = await _presenter.GetSearchedPosts();
            if (errors != null && errors.Count != 0)
                ShowAlert(errors[0]);

            if (_bar != null)
            {
                _bar.Visibility = ViewStates.Gone;
                _refresher.Refreshing = false;
            }
            _feedAdapter?.NotifyDataSetChanged();
        }

        public void OnSearchPosts(string title, PostType type)
        {
            Title.Text = title;
            _bar.Visibility = ViewStates.Visible;
            _presenter.PostType = type;
            _presenter.Tag = null;
            _presenter.ClearPosts();
            _scrollListner.ClearPosition();
            LoadPosts();
        }

        public void PhotoClick(int position)
        {
            var intent = new Intent(Context, typeof(PostPreviewActivity));
            intent.PutExtra("PhotoURL", _presenter.Posts[position].Body);
            StartActivity(intent);
        }

        void FeedAdapter_CommentAction(int position)
        {
            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra("uid", _presenter.Posts[position].Url);
            Context.StartActivity(intent);
        }

        void FeedAdapter_VotersAction(int position)
        {
            Activity.Intent.PutExtra("url", _presenter.Posts[position].Url);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        void FeedAdapter_UserAction(int position)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_presenter.Posts[position].Author));
        }

        private async void FeedAdapter_LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await _presenter.Vote(position);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors[0]);

                _feedAdapter?.NotifyDataSetChanged();
            }
            else
                OpenLogin();
        }

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
