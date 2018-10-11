using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Facades;
using Steepshot.Core.Utils;

namespace Steepshot.Fragment
{
    public sealed class SearchFragment : BaseFragment
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
        private BrowseSearchTagsAdapter _categoriesAdapter;
        private FollowersAdapter _usersSearchAdapter;
        private SearchFacade _searchFacade;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.categories)] private RecyclerView _categories;
        [BindView(Resource.Id.users)] private RecyclerView _users;
        [BindView(Resource.Id.search_view)] private EditText _searchView;
        [BindView(Resource.Id.people_loading_spinner)] private ProgressBar _peopleSpinner;
        [BindView(Resource.Id.tag_loading_spinner)] private ProgressBar _tagSpinner;
        [BindView(Resource.Id.tags_button)] private Button _tagsButton;
        [BindView(Resource.Id.people_button)] private Button _peopleButton;
        [BindView(Resource.Id.clear_button)] private Button _clearButton;
        [BindView(Resource.Id.tags_layout)] private RelativeLayout _tagsLayout;
        [BindView(Resource.Id.users_layout)] private RelativeLayout _usersLayout;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (_searchFacade == null)
                _searchFacade = App.Container.GetFacade<SearchFacade>(App.MainChain);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_search, null);
                Cheeseknife.Bind(this, InflatedView);
            }
            ToggleTabBar();
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            _searchView.Hint = App.Localization.GetText(LocalizationKeys.SearchHint);
            _searchView.SetFilters(new IInputFilter[] { new TextInputFilter(TextInputFilter.TagFilter) });
            _tagsButton.Text = App.Localization.GetText(LocalizationKeys.Tag);
            _peopleButton.Text = App.Localization.GetText(LocalizationKeys.Users);
            _clearButton.Text = App.Localization.GetText(LocalizationKeys.Clear);
            _emptyQueryLabel.Text = App.Localization.GetText(LocalizationKeys.EmptyQuery);

            _searchView.TextChanged += OnSearchViewOnTextChanged;

            _categories.SetLayoutManager(new LinearLayoutManager(Activity));
            _users.SetLayoutManager(new LinearLayoutManager(Activity));

            _searchFacade.UserFriendPresenter.SourceChanged += UserFriendPresenterSourceChanged;
            _searchFacade.TagsPresenter.SourceChanged += TagsPresenterSourceChanged;
            _categoriesAdapter = new BrowseSearchTagsAdapter(_searchFacade.TagsPresenter);
            _usersSearchAdapter = new FollowersAdapter(Activity, _searchFacade.UserFriendPresenter);
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
            _emptyQueryLabel.Visibility = ViewStates.Invisible;
        }

        public override void OnResume()
        {
            base.OnResume();
            ToggleTabBar(true);
        }
        
        private void OnClearClick(object sender, EventArgs e)
        {
            _searchView.Text = string.Empty;
            _emptyQueryLabel.Visibility = ViewStates.Invisible;
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

            _timer.Change(1300, Timeout.Infinite);
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

            var result = await _searchFacade.UserFriendPresenter.TryFollowAsync(userFriend);
            if (!IsInitialized)
                return;

            Context.ShowAlert(result);
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
                    _searchFacade.UserFriendPresenter.Clear();
                else
                    _searchFacade.TagsPresenter.Clear();

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

            var exception = await _searchFacade.TrySearchCategoriesAsync(_searchView.Text, _searchType);
            if (!IsInitialized)
                return;
            CheckQueryIsEmpty();
            Context.ShowAlert(exception, ToastLength.Short);
        }

        private void CheckQueryIsEmpty()
        {
            if (string.IsNullOrEmpty(_searchView.Text))
                return;

            if (_searchType == SearchType.People)
                _emptyQueryLabel.Visibility =
                    _searchFacade.UserFriendPresenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
            else
                _emptyQueryLabel.Visibility = _searchFacade.TagsPresenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
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

        public override void OnDetach()
        {
            _searchFacade.TasksCancel();
            _categories.SetAdapter(null);
            _users.SetAdapter(null);
            base.OnDetach();
        }
    }
}
