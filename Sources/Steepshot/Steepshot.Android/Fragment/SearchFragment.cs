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
using Steepshot.Core.Utils;
using Steepshot.Presenter;

using SearchView = Steepshot.View.SearchView;

namespace Steepshot.Fragment
{
	public class SearchFragment : BaseFragment, SearchView
    {
		private Timer _timer;
		SearchPresenter presenter;
		private SearchType _searchType = SearchType.Tags;
		private CancellationTokenSource cts;
		private Dictionary<SearchType, string> prevQuery = new Dictionary<SearchType, string>() { { SearchType.People, null }, { SearchType.Tags, null } };

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.categories)] RecyclerView categories;
		[InjectView(Resource.Id.users)] RecyclerView users;
        [InjectView(Resource.Id.search_view)] Android.Support.V7.Widget.SearchView searchView;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar spinner;
		[InjectView(Resource.Id.tags_button)] Button tagsButton;
		[InjectView(Resource.Id.people_button)] Button peopleButton;
#pragma warning restore 0649

        CategoriesAdapter _categoriesAdapter;
		UsersSearchAdapter _usersSearchAdapter;
		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!_isInitialized)
			{
				v = inflater.Inflate(Resource.Layout.lyt_search, null);
				Cheeseknife.Inject(this, v);
			}
			return v;
		}

		public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
		{
			if (_isInitialized)
				return;
			
			base.OnViewCreated(view, savedInstanceState);
			searchView.QueryTextChange += (sender, e) =>
			{
				_timer.Change(500, Timeout.Infinite);
			};

			categories.SetLayoutManager(new LinearLayoutManager(Activity));
			users.SetLayoutManager(new LinearLayoutManager(Activity));

			_categoriesAdapter = new CategoriesAdapter(Activity);
			_usersSearchAdapter = new UsersSearchAdapter(Activity);
			categories.SetAdapter(_categoriesAdapter);
			users.SetAdapter(_usersSearchAdapter);

			_categoriesAdapter.Click += OnClick;
			_usersSearchAdapter.Click += OnClick;
			_timer = new Timer(onTimer);
			searchView.Iconified = false;
			searchView.ClearFocus();
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
			if (_searchType == SearchType.Tags)
			{
				Activity.Intent.PutExtra("SEARCH", _categoriesAdapter.GetItem(pos).Name);

				if (Activity.CurrentFocus != null)
				{
					InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
					imm.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, 0);
				}
				Activity.OnBackPressed();
			}
			else
			{
				((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_usersSearchAdapter.Items[pos].Username));
			}
        }

		private void onTimer(object state)
		{
			Activity.RunOnUiThread(() =>
		   {
			   _usersSearchAdapter.Items.Clear();
			   GetTags();
		   });
		}

		Task<OperationResult> tagsTask;
		private async Task GetTags()
		{
			try
			{
				var query = searchView.Query;
				if (prevQuery[_searchType] == query)
					return;
				if ((query != null && (query.Length == 1 || (query.Length == 2 && _searchType == SearchType.People))) || (string.IsNullOrEmpty(query) && _searchType == SearchType.People))
					return;
				prevQuery[_searchType] = query;
				spinner.Visibility = ViewStates.Visible;
				tagsTask = presenter.SearchCategories(searchView.Query, _searchType);

				if (_searchType == SearchType.Tags)
				{
					var tags = (OperationResult<SearchResponse<SearchResult>>)await tagsTask;
					if (tags?.Result?.Results != null)
					{
						_categoriesAdapter.Reset(tags.Result.Results);
						_categoriesAdapter.NotifyDataSetChanged();
					}
				}
				else
				{
					var usersList = (OperationResult<UserSearchResponse>)await tagsTask;
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
				if (spinner != null)
					spinner.Visibility = ViewStates.Gone;
			}
		}

		protected override void CreatePresenter()
		{
			presenter = new SearchPresenter(this);
		}

		private void SwitchSearchType()
		{
			GetTags();
			if (_searchType == SearchType.Tags)
			{
				users.Visibility = ViewStates.Gone;
				categories.Visibility = ViewStates.Visible;
				tagsButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
				tagsButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 17);

				peopleButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
				peopleButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
			}
			else
			{
				users.Visibility = ViewStates.Visible;
				categories.Visibility = ViewStates.Gone;
				peopleButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
				peopleButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 17);

				tagsButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
				tagsButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
			}
		}

		public override void OnDetach()
		{
			base.OnDetach();
			Cheeseknife.Reset(this);
		}
	}

	public enum SearchType
	{
		Tags,
		People
	}
}
