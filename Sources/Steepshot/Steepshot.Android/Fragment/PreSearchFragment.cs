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

        public string CustomTag
        {
            get => _presenter.Tag;
            set => _presenter.Tag = value;
        }

        ProfileFeedAdapter _profileFeedAdapter;
        ProfileFeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new ProfileFeedAdapter(Context, _presenter, new[] { _font, _semiboldFont }, false);
                    _profileFeedAdapter.PhotoClick += OnPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                }
                return _profileFeedAdapter;
            }
        }

        ProfileGridAdapter _profileGridAdapter;
        ProfileGridAdapter ProfileGridAdapter
        {
            get
            {
                if (_profileGridAdapter == null)
                {
                    _profileGridAdapter = new ProfileGridAdapter(Context, _presenter, new[] { _font, _semiboldFont }, false);
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
            if (_searchList.GetLayoutManager() is GridLayoutManager)
            {
                _switcher.SetImageResource(Resource.Drawable.grid);
                _searchList.SetLayoutManager(_linearLayoutManager);
                _searchList.SetAdapter(ProfileFeedAdapter);
            }
            else
            {
                _switcher.SetImageResource(Resource.Drawable.grid_active);
                _searchList.SetLayoutManager(_gridLayoutManager);
                _searchList.AddItemDecoration(new ProfileGridItemdecoration(1));
                _searchList.SetAdapter(ProfileGridAdapter);
            }
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
            try
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

            _font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            _semiboldFont = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");

            _searchView.Typeface = _font;
            _clearButton.Typeface = _font;

            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += async () => await LoadPosts();

            _linearLayoutManager = new LinearLayoutManager(Context);
            _gridLayoutManager = new GridLayoutManager(Context, 3);

            _searchList.SetLayoutManager(_gridLayoutManager);
            _searchList.AddItemDecoration(new ProfileGridItemdecoration(1));
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
                var errors = await _presenter.Vote(position);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors);

                _searchList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else
            {
                var intent = new Intent(Context, typeof(PreSignInActivity));
                StartActivity(intent);
            }
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
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private async Task LoadPosts(bool clearOld = false)
        {
            if (_spinner != null)
                _spinner.Visibility = ViewStates.Visible;

            try
            {
                List<string> errors;
                if (string.IsNullOrEmpty(CustomTag))
                    errors = await _presenter.GetTopPosts(clearOld);
                else
                    errors = await _presenter.GetSearchedPosts(clearOld);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors);

                if (_spinner != null)
                    _spinner.Visibility = ViewStates.Gone;

                _searchList?.GetAdapter()?.NotifyDataSetChanged();
            }
            catch (Exception)
            {
                //Catching rethrowed task canceled exception from presenter
            }
        }

        protected override void CreatePresenter()
        {
            _presenter = new PreSearchPresenter();
        }

        private void SwitchSearchType(PostType postType)
        {
            _presenter.PostType = postType;
            switch (postType)
            {
                case PostType.Top:
                    _trendingButton.Typeface = _semiboldFont;
                    _trendingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
                    _trendingButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30)));

                    _hotButton.Typeface = _font;
                    _hotButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                    _hotButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));

                    _newButton.Typeface = _font;
                    _newButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                    _newButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));
                    break;
                case PostType.Hot:
                    _hotButton.Typeface = _semiboldFont;
                    _hotButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
                    _hotButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30)));

                    _trendingButton.Typeface = _font;
                    _trendingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                    _trendingButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));

                    _newButton.Typeface = _font;
                    _newButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                    _newButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));
                    break;
                case PostType.New:
                    _newButton.Typeface = _semiboldFont;
                    _newButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
                    _newButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30)));

                    _hotButton.Typeface = _font;
                    _hotButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                    _hotButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));

                    _trendingButton.Typeface = _font;
                    _trendingButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                    _trendingButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));
                    break;
            }
            LoadPosts(true);
        }
    }
}
