using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
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
    public class PreSearchFragment : BaseFragmentWithPresenter<PreSearchPresenter>
    {
        private Typeface _font, _semiboldFont;
        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;

        public string CustomTag
        {
            get => _presenter.Tag;
            set => _presenter.Tag = value;
        }

        FeedAdapter _profileFeedAdapter;
        FeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new FeedAdapter(Context, _presenter, new[] { _font, _semiboldFont });
                    _profileFeedAdapter.PhotoClick += OnPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                }
                return _profileFeedAdapter;
            }
        }

        GridAdapter _profileGridAdapter;
        GridAdapter ProfileGridAdapter
        {
            get
            {
                if (_profileGridAdapter == null)
                {
                    _profileGridAdapter = new GridAdapter(Context, _presenter);
                    _profileGridAdapter.Click += OnPhotoClick;
                }
                return _profileGridAdapter;
            }
        }

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.search_list)] RecyclerView _searchList;
        [InjectView(Resource.Id.search_view)] TextView _searchView;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar _spinner;
        [InjectView(Resource.Id.trending_button)] Button _trendingButton;
        [InjectView(Resource.Id.hot_button)] Button _hotButton;
        [InjectView(Resource.Id.new_button)] Button _newButton;
        [InjectView(Resource.Id.clear_button)] Button _clearButton;
        [InjectView(Resource.Id.btn_switcher)] ImageButton _switcher;
        [InjectView(Resource.Id.refresher)] SwipeRefreshLayout _refresher;
        [InjectView(Resource.Id.login)] Button _loginButton;
#pragma warning restore 0649

        [InjectOnClick(Resource.Id.clear_button)]
        public void OnClearClick(object sender, EventArgs e)
        {
            CustomTag = null;
            _clearButton.Visibility = ViewStates.Visible;
            _searchView.Text = "Tap to search";
            _searchView.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));
        }

        [InjectOnClick(Resource.Id.trending_button)]
        public void OnTrendClick(object sender, EventArgs e)
        {
            SwitchSearchType(PostType.Top);
        }

        [InjectOnClick(Resource.Id.hot_button)]
        public void OnTopClick(object sender, EventArgs e)
        {
            SwitchSearchType(PostType.Hot);
        }

        [InjectOnClick(Resource.Id.new_button)]
        public void OnNewClick(object sender, EventArgs e)
        {
            SwitchSearchType(PostType.New);
        }

        [InjectOnClick(Resource.Id.toolbar)]
        public void OnSearch(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new SearchFragment());
        }

        [InjectOnClick(Resource.Id.btn_switcher)]
        public void OnSwitcherClick(object sender, EventArgs e)
        {
            _scrollListner.ClearPosition();
            if (_searchList.GetLayoutManager() is GridLayoutManager)
            {
                _switcher.SetImageResource(Resource.Drawable.grid);
                _searchList.SetLayoutManager(_linearLayoutManager);
                _searchList.RemoveItemDecoration(_gridItemDecoration);
                _searchList.SetAdapter(ProfileFeedAdapter);
            }
            else
            {
                _switcher.SetImageResource(Resource.Drawable.grid_active);
                _searchList.SetLayoutManager(_gridLayoutManager);
                _searchList.AddItemDecoration(_gridItemDecoration);
                _searchList.SetAdapter(ProfileGridAdapter);
            }
        }

        [InjectOnClick(Resource.Id.login)]
        public void OnLogin(object sender, EventArgs e)
        {
            OpenLogin();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                V = inflater.Inflate(Resource.Layout.lyt_presearch, null);
                Cheeseknife.Inject(this, V);
            }
            return V;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try //TODO:KOA: is try catch realy needed?
            {
                var s = Activity.Intent.GetStringExtra("SEARCH");
                if (s != null && s != CustomTag && _spinner != null)
                {
                    Activity.Intent.RemoveExtra("SEARCH");
                    _searchView.Text = _presenter.Tag = CustomTag = s;
                    _searchView.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30)));
                    _spinner.Visibility = ViewStates.Visible;
                    _clearButton.Visibility = ViewStates.Visible;
                    _scrollListner.ClearPosition();
                    LoadPosts(true);
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }

            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            if (BasePresenter.User.IsAuthenticated)
                _loginButton.Visibility = ViewStates.Gone;

            _font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            _semiboldFont = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");

            _searchView.Typeface = _font;
            _clearButton.Typeface = _font;
            _loginButton.Typeface = _semiboldFont;
            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += async () => await LoadPosts();

            _linearLayoutManager = new LinearLayoutManager(Context);
            _gridLayoutManager = new GridLayoutManager(Context, 3);

            _gridItemDecoration = new GridItemDecoration();
            _searchList.SetLayoutManager(_gridLayoutManager);
            _searchList.AddItemDecoration(_gridItemDecoration);
            _searchList.AddOnScrollListener(_scrollListner);
            _searchList.SetAdapter(ProfileGridAdapter);
            SwitchSearchType(PostType.Top);

            _refresher.Refresh += async delegate
            {
                await LoadPosts(true);
                _refresher.Refreshing = false;
            };
        }

        public void OnPhotoClick(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            var photo = post.Photos?.FirstOrDefault();
            if (photo != null)
            {
                var intent = new Intent(Context, typeof(PostPreviewActivity));
                intent.PutExtra("PhotoURL", photo);
                StartActivity(intent);
            }
        }

        private async void LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await _presenter.TryVote(position);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors);

                _searchList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else
                OpenLogin();

        }

        private void UserAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            if (BasePresenter.User.Login != post.Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private void CommentAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra("uid", post.Url);
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

        private async Task LoadPosts(bool clearOld = false)
        {
            if (_spinner != null)
                _spinner.Visibility = ViewStates.Visible;

            if (clearOld)
            {
                _presenter.LoadCancel();
                _presenter.Clear();
                _scrollListner.ClearPosition();
            }

            List<string> errors;
            if (string.IsNullOrEmpty(CustomTag))
                errors = await _presenter.TryLoadNextTopPosts();
            else
                errors = await _presenter.TryGetSearchedPosts();

            if (errors != null)
            {
                if (errors.Any())
                    ShowAlert(errors);
                else
                    _searchList?.GetAdapter()?.NotifyDataSetChanged();
            }

            if (_spinner != null)
                _spinner.Visibility = ViewStates.Gone;
        }

        protected override void CreatePresenter()
        {
            _presenter = new PreSearchPresenter();
        }

        private void OpenLogin()
        {
            var intent = new Intent(Activity, typeof(PreSignInActivity));
            StartActivity(intent);
        }

        private void SwitchSearchType(PostType postType)
        {
            Button btn1, btn2, btn3;
            switch (postType)
            {
                case PostType.Hot:
                    {
                        btn1 = _hotButton;
                        btn2 = _trendingButton;
                        btn3 = _newButton;
                        break;
                    }
                case PostType.New:
                    {
                        btn1 = _newButton;
                        btn2 = _hotButton;
                        btn3 = _trendingButton;
                        break;
                    }
                default:
                    {
                        btn1 = _trendingButton;
                        btn2 = _hotButton;
                        btn3 = _newButton;
                        break;
                    }
            }

            var clMain = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30));
            var clPrim = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158));

            btn1.Typeface = _semiboldFont;
            btn1.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
            btn1.SetTextColor(clMain);

            btn2.Typeface = _font;
            btn2.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
            btn2.SetTextColor(clPrim);

            btn3.Typeface = _font;
            btn3.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
            btn3.SetTextColor(clPrim);

            _presenter.PostType = postType;
            LoadPosts(true);
        }
    }
}
