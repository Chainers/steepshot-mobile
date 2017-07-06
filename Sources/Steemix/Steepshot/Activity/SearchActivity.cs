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

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.categories)] RecyclerView categories;
        [InjectView(Resource.Id.search_view)] Android.Support.V7.Widget.SearchView searchView;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar spinner;
		[InjectView(Resource.Id.tags_button)] Button tagsButton;
		[InjectView(Resource.Id.people_button)] Button peopleButton;
#pragma warning restore 0649

        CategoriesAdapter Adapter;

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

            Adapter = new CategoriesAdapter(this);

            categories.SetAdapter(Adapter);

            Adapter.Click += OnClick;
			_timer = new Timer(onTimer);
            searchView.Iconified = false;
            searchView.ClearFocus();
			SwitchSearchType();
            GetTags();
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
				b.PutString("SEARCH", Adapter.GetItem(pos).Name);
				returnIntent.PutExtra("SEARCH", b);
				SetResult(Result.Ok, returnIntent);
				Finish();
			}
			else
			{
				Intent intent = new Intent(this, typeof(ProfileActivity));
				intent.PutExtra("ID", Adapter.GetItem(pos).Name);
				this.StartActivity(intent);
			}
        }

		private void onTimer(object state)
		{
			RunOnUiThread(() =>
		   {
			   GetTags();
		   });
		}

		Task<OperationResult<SearchResponse>> tagsTask;
		private async Task GetTags()
		{
			try
			{
				var query = searchView.Query;
				if (_prevQuery == query)
					return;
				if ((query != null && (query.Length == 1 || (query.Length == 2 && _searchType == SearchType.People))) || (string.IsNullOrEmpty(query) && _searchType == SearchType.People))
					return;
				_prevQuery = query;
				spinner.Visibility = ViewStates.Visible;
				tagsTask = presenter.SearchCategories(searchView.Query, _searchType);
				var tags = await tagsTask;
				if (tags?.Result?.Results != null)
				{
					Adapter.Reset(tags.Result.Results);
					Adapter.NotifyDataSetChanged();
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
			Adapter.Reset(new List<SearchResult>());
			_prevQuery = null;
			Adapter.NotifyDataSetChanged();
			GetTags();
			if (_searchType == SearchType.Tags)
			{
				tagsButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
				tagsButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 17);

				peopleButton.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
				peopleButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 15);
			}
			else
			{
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