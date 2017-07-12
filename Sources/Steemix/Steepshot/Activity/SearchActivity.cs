using System;
using Android.Content.PM;
using Android.App;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Content;
using System.Threading;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Responses;
using Android.Widget;
using System.Collections.Generic;

namespace Steepshot
{
    [Activity(Label = "SearchActivity", ScreenOrientation = ScreenOrientation.Portrait)]
	public class SearchActivity : BaseActivity,SearchView
    {
		private Timer _timer;
		SearchPresenter presenter;
		private SearchType _searchType = SearchType.Tags;
		private string _prevQuery;
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_search);
            Cheeseknife.Inject(this);

			searchView.QueryTextChange += (sender, e) =>
			{
				_timer.Change(500, Timeout.Infinite);
			};

            categories.SetLayoutManager(new LinearLayoutManager(this));
			users.SetLayoutManager(new LinearLayoutManager(this));

            _categoriesAdapter = new CategoriesAdapter(this);
			_usersSearchAdapter = new UsersSearchAdapter(this);
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
				Intent returnIntent = new Intent();
				Bundle b = new Bundle();
				b.PutString("SEARCH", _categoriesAdapter.GetItem(pos).Name);
				returnIntent.PutExtra("SEARCH", b);
				SetResult(Result.Ok, returnIntent);
				Finish();
			}
			else
			{
				Intent intent = new Intent(this, typeof(ProfileActivity));
				intent.PutExtra("ID", _usersSearchAdapter.Items[pos].Username);
				this.StartActivity(intent);
			}
        }

		private void onTimer(object state)
		{
			RunOnUiThread(() =>
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
				Reporter.SendCrash(ex);
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

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Cheeseknife.Reset(this);
		}
	}

	public enum SearchType
	{
		Tags,
		People
	}
}