using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Android.Support.Design.Widget;
using Steepshot.Activity;

namespace Steepshot.Fragment
{
    public sealed class SearchFragment : BaseFragmentWithPresenter<SearchPresenter>
    {
        public const string SearchExtra = "SEARCH";

        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string>
        {
            {SearchType.People, null},
            {SearchType.Tags, null}
        };

        private Timer _timer;
        private SearchType _searchType = SearchType.Tags;
        private ScrollListener _scrollListner;
        private TagsAdapter _categoriesAdapter;
        private FollowersAdapter _usersSearchAdapter;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.categories)] private RecyclerView _categories;
        [InjectView(Resource.Id.users)] private RecyclerView _users;
        [InjectView(Resource.Id.search_view)] private EditText _searchView;
        [InjectView(Resource.Id.people_loading_spinner)] private ProgressBar _peopleSpinner;
        [InjectView(Resource.Id.tag_loading_spinner)] private ProgressBar _tagSpinner;
        [InjectView(Resource.Id.tags_button)] private Button _tagsButton;
        [InjectView(Resource.Id.people_button)] private Button _peopleButton;
        [InjectView(Resource.Id.clear_button)] private Button _clearButton;
        [InjectView(Resource.Id.tags_layout)] private RelativeLayout _tagsLayout;
        [InjectView(Resource.Id.users_layout)] private RelativeLayout _usersLayout;
        [InjectView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_search, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            ToggleTabBar();
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            _searchView.TextChanged += OnSearchViewOnTextChanged;

            _categories.SetLayoutManager(new LinearLayoutManager(Activity));
            _users.SetLayoutManager(new LinearLayoutManager(Activity));

            Presenter.UserFriendPresenter.SourceChanged += UserFriendPresenterSourceChanged;
            Presenter.TagsPresenter.SourceChanged += TagsPresenterSourceChanged;
            _categoriesAdapter = new TagsAdapter(Presenter.TagsPresenter);
            _usersSearchAdapter = new FollowersAdapter(Activity, Presenter.UserFriendPresenter);
            _categories.SetAdapter(_categoriesAdapter);
            _users.SetAdapter(_usersSearchAdapter);

            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += GetTags;
            _users.AddOnScrollListener(_scrollListner);

            _categoriesAdapter.Click += OnClick;
            _usersSearchAdapter.UserAction += OnClick;
            _usersSearchAdapter.FollowAction += Follow;
            _timer = new Timer(OnTimer);

            _searchView.Typeface = Style.Regular;
            _clearButton.Typeface = Style.Regular;
            _clearButton.Visibility = ViewStates.Gone;
            _clearButton.Click += OnClearClick;
            _tagsButton.Click += TagsClick;
            _peopleButton.Click += PeopleClick;
            SwitchSearchType(false);
            _searchView.RequestFocus();

            ((BaseActivity)Activity).OpenKeyboard(_searchView);

            _emptyQueryLabel.Typeface = Style.Light;
            _emptyQueryLabel.Text = Localization.Texts.EmptyQuery;
            _emptyQueryLabel.Visibility = ViewStates.Invisible;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        private void OnClearClick(object sender, EventArgs e)
        {
            _searchView.Text = string.Empty;
        }

        private void TagsClick(object sender, EventArgs e)
        {
            _searchType = SearchType.Tags;
            SwitchSearchType();
        }

        private void PeopleClick(object sender, EventArgs e)
        {
            _searchType = SearchType.People;
            SwitchSearchType();
        }

        private void UserFriendPresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _peopleSpinner.Visibility = ViewStates.Gone;
                _usersSearchAdapter.NotifyDataSetChanged();
            });
        }

        private void TagsPresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _tagSpinner.Visibility = ViewStates.Gone;
                _categoriesAdapter.NotifyDataSetChanged();
            });
        }

        private void OnSearchViewOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _clearButton.Visibility = string.IsNullOrEmpty(e.Text.ToString())
                ? ViewStates.Gone
                : ViewStates.Visible;

            _timer.Change(500, Timeout.Infinite);
        }

        private void OnClick(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            ((BaseActivity)Activity).HideKeyboard();
            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(userFriend.Author));
        }


        private void OnClick(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            ((BaseActivity)Activity).HideKeyboard();
            Activity.Intent.PutExtra(SearchExtra, tag);
            ((BaseActivity)Activity).OpenNewContentFragment(new PreSearchFragment());
        }

        private async void Follow(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            var errors = await Presenter.UserFriendPresenter.TryFollow(userFriend);
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors);
        }

        private void OnTimer(object state)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                GetTags(true);
            });
        }

        private async void GetTags()
        {
            await GetTags(false, false);
        }

        private async Task GetTags(bool clear, bool isLoaderNeeded = true)
        {
            CheckQueryIsEmpty();
            if (clear)
            {
                if (_prevQuery.ContainsKey(_searchType) && string.Equals(_prevQuery[_searchType], _searchView.Text, StringComparison.OrdinalIgnoreCase))
                    return;

                if (_searchType == SearchType.People)
                    Presenter.UserFriendPresenter.Clear();
                else
                    Presenter.TagsPresenter.Clear();

                _scrollListner.ClearPosition();

                if (_prevQuery.ContainsKey(_searchType))
                    _prevQuery[_searchType] = _searchView.Text;
                else
                    _prevQuery.Add(_searchType, _searchView.Text);
            }

            if (isLoaderNeeded)
            {
                _emptyQueryLabel.Visibility = ViewStates.Invisible;
                if (_searchType == SearchType.People)
                    _peopleSpinner.Visibility = ViewStates.Visible;
                else
                    _tagSpinner.Visibility = ViewStates.Visible;
            }

            var errors = await Presenter.TrySearchCategories(_searchView.Text, _searchType);
            if (!IsInitialized)
                return;
            CheckQueryIsEmpty();
            Context.ShowAlert(errors, ToastLength.Short);
        }

        private void CheckQueryIsEmpty()
        {
            if (string.IsNullOrEmpty(_searchView.Text))
                return;

            if (_searchType == SearchType.People)
                _emptyQueryLabel.Visibility =
                    Presenter.UserFriendPresenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
            else
                _emptyQueryLabel.Visibility = Presenter.TagsPresenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        private async void SwitchSearchType(bool isLoaderNeeded = true)
        {
            var btMain = _peopleButton;
            var btSecond = _tagsButton;
            var rvMain = _usersLayout;
            var rvSecond = _tagsLayout;

            if (_searchType == SearchType.Tags)
            {
                btMain = _tagsButton;
                btSecond = _peopleButton;
                rvMain = _tagsLayout;
                rvSecond = _usersLayout;
            }

            rvMain.Visibility = ViewStates.Visible;
            rvSecond.Visibility = ViewStates.Gone;

            btMain.Typeface = Style.Semibold;
            btMain.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
            btMain.SetTextColor(Style.R15G24B30);

            btSecond.Typeface = Style.Regular;
            btSecond.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
            btSecond.SetTextColor(Style.R151G155B158);

            await GetTags(true, isLoaderNeeded);
        }
    }
}
