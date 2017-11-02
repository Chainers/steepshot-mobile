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
using Steepshot.Core.Presenters;
using Steepshot.Utils;

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
        private SearchType _searchType = SearchType.People;
        private ScrollListener _scrollListner;
        private TagsAdapter _categoriesAdapter;
        private FollowersAdapter _usersSearchAdapter;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.categories)] private RecyclerView _categories;
        [InjectView(Resource.Id.users)] private RecyclerView _users;
        [InjectView(Resource.Id.search_view)] private EditText _searchView;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.tags_button)] private Button _tagsButton;
        [InjectView(Resource.Id.people_button)] private Button _peopleButton;
        [InjectView(Resource.Id.clear_button)] private Button _clearButton;
#pragma warning restore 0649


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_search, null);
                Cheeseknife.Inject(this, InflatedView);
            }
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
            SwitchSearchType();
            _searchView.RequestFocus();

            var imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(_searchView, ShowFlags.Implicit);
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

        private void UserFriendPresenterSourceChanged()
        {
            if (!IsInitialized || IsDetached || IsRemoving)
                return;

            Activity.RunOnUiThread(() =>
            {
                _usersSearchAdapter.NotifyDataSetChanged();
            });
        }

        private void TagsPresenterSourceChanged()
        {
            if (!IsInitialized || IsDetached || IsRemoving)
                return;

            Activity.RunOnUiThread(() =>
            {
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

        private void OnClick(int pos)
        {
            if (Activity.CurrentFocus != null)
            {
                var imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, 0);
            }
            if (_searchType == SearchType.Tags)
            {
                var user = Presenter.TagsPresenter[pos];
                if (user == null)
                    return;

                Activity.Intent.PutExtra(SearchExtra, user.Name);
                Activity.OnBackPressed();
            }
            else if (_searchType == SearchType.People)
            {

                if (Presenter.UserFriendPresenter.Count > pos)
                {
                    var user = Presenter.UserFriendPresenter[pos];
                    if (user == null)
                        return;
                    ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(user.Author));
                }
            }
        }

        private async void Follow(int position)
        {
            var user = Presenter.UserFriendPresenter[position];
            if (user == null)
                return;

            var errors = await Presenter.UserFriendPresenter.TryFollow(user);
            if (!IsInitialized || IsDetached || IsRemoving)
                return;

            Context.ShowAlert(errors);
        }

        private void OnTimer(object state)
        {
            if (!IsInitialized || IsDetached || IsRemoving)
                return;

            Activity.RunOnUiThread(() =>
            {
                GetTags(true);
            });
        }

        private async void GetTags()
        {
            await GetTags(false);
        }

        private async Task GetTags(bool clear)
        {
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

            _spinner.Visibility = ViewStates.Visible;

            var errors = await Presenter.TrySearchCategories(_searchView.Text, _searchType);
            if (!IsInitialized || IsDetached || IsRemoving)
                return;

            Context.ShowAlert(errors, ToastLength.Short);
            _spinner.Visibility = ViewStates.Gone;
        }

        private async void SwitchSearchType()
        {
            var btMain = _peopleButton;
            var btSecond = _tagsButton;
            var rvmain = _users;
            var rvSecond = _categories;

            if (_searchType == SearchType.Tags)
            {
                btMain = _tagsButton;
                btSecond = _peopleButton;
                rvmain = _categories;
                rvSecond = _users;
            }

            rvmain.Visibility = ViewStates.Visible;
            rvSecond.Visibility = ViewStates.Gone;

            btMain.Typeface = Style.Semibold;
            btMain.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
            btMain.SetTextColor(Style.R15G24B30);

            btSecond.Typeface = Style.Regular;
            btSecond.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
            btSecond.SetTextColor(Style.R151G155B158);

            await GetTags(true);
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }
    }
}
