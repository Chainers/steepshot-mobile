using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.Fragment
{
    public class SearchFragment : BaseFragment
    {
        private Timer _timer;
        SearchPresenter _presenter;
        private SearchType _searchType = SearchType.Tags;
        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string> { { SearchType.People, null }, { SearchType.Tags, null } };

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.categories)] RecyclerView _categories;
        [InjectView(Resource.Id.users)] RecyclerView _users;
        [InjectView(Resource.Id.search_view)] Android.Support.V7.Widget.SearchView _searchView;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar _spinner;
        [InjectView(Resource.Id.tags_button)] Button _tagsButton;
        [InjectView(Resource.Id.people_button)] Button _peopleButton;
#pragma warning restore 0649

        CategoriesAdapter _categoriesAdapter;
        UsersSearchAdapter _usersSearchAdapter;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                V = inflater.Inflate(Resource.Layout.lyt_search, null);
                Cheeseknife.Inject(this, V);
            }
            return V;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);
            _searchView.QueryTextChange += (sender, e) =>
            {
                _timer.Change(500, Timeout.Infinite);
            };

            _categories.SetLayoutManager(new LinearLayoutManager(Activity));
            _users.SetLayoutManager(new LinearLayoutManager(Activity));

            _categoriesAdapter = new CategoriesAdapter();
            _usersSearchAdapter = new UsersSearchAdapter(Activity);
            _categories.SetAdapter(_categoriesAdapter);
            _users.SetAdapter(_usersSearchAdapter);

            _categoriesAdapter.Click += OnClick;
            _usersSearchAdapter.Click += OnClick;
            _timer = new Timer(OnTimer);
            _searchView.Iconified = false;
            _searchView.ClearFocus();
            SwitchSearchType();
        }

        [InjectOnClick(Resource.Id.tags_button)]
        public void TagsClick(object sender, EventArgs e)
        {
            _searchType = SearchType.Tags;
            SwitchSearchType();
        }

        [InjectOnClick(Resource.Id.people_button)]
        public void PeopleClick(object sender, EventArgs e)
        {
            _searchType = SearchType.People;
            SwitchSearchType();
        }

        public void OnClick(int pos)
        {
			if (Activity.CurrentFocus != null)
			{
				var imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
				imm.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, 0);
			}
            if (_searchType == SearchType.Tags)
            {
                Activity.Intent.PutExtra("SEARCH", _categoriesAdapter.GetItem(pos).Name);
                Activity.OnBackPressed();
            }
            else
            {
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_usersSearchAdapter.Items[pos].Username));
            }
        }

        private void OnTimer(object state)
        {
            Activity.RunOnUiThread(() =>
           {
               _usersSearchAdapter.Items.Clear();
               GetTags();
           });
        }

        private async Task GetTags()
        {
            try
            {
                var query = _searchView.Query;
                if (_prevQuery[_searchType] == query)
                    return;

                if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && _searchType == SearchType.People))
                    || string.IsNullOrEmpty(query) && _searchType == SearchType.People)
                    return;

                _prevQuery[_searchType] = query;
                _spinner.Visibility = ViewStates.Visible;
                var tagsTask = await _presenter.SearchCategories(_searchView.Query, _searchType);
                if (_searchType == SearchType.Tags)
                {
                    var tags = (OperationResult<SearchResponse<SearchResult>>)tagsTask;
                    if (tags?.Result?.Results != null)
                    {
                        _categoriesAdapter.Reset(tags.Result.Results);
                        _categoriesAdapter.NotifyDataSetChanged();
                    }
                }
                else
                {
                    var usersList = (OperationResult<SearchResponse<UserSearchResult>>)tagsTask;
                    if (usersList?.Result?.Results != null)
                    {
                        _usersSearchAdapter.Items = usersList.Result.Results;
                        _usersSearchAdapter.NotifyDataSetChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
            }
            finally
            {
                if (_spinner != null)
                    _spinner.Visibility = ViewStates.Gone;
            }
        }

        protected override void CreatePresenter()
        {
            _presenter = new SearchPresenter();
        }

        private void SwitchSearchType()
        {
            GetTags();
            if (_searchType == SearchType.Tags)
            {
                _users.Visibility = ViewStates.Gone;
                _categories.Visibility = ViewStates.Visible;
                _tagsButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                _tagsButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 17);

                _peopleButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
                _peopleButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
            }
            else
            {
                _users.Visibility = ViewStates.Visible;
                _categories.Visibility = ViewStates.Gone;
                _peopleButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                _peopleButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 17);

                _tagsButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
                _tagsButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
            }
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }
    }
}