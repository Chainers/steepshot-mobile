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

namespace Steepshot
{
    [Activity(Label = "SearchActivity", ScreenOrientation = ScreenOrientation.Portrait)]
	public class SearchActivity : BaseActivity,SearchView
    {
		private Timer _timer;
		SearchPresenter presenter;
#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.categories)] RecyclerView categories;
        [InjectView(Resource.Id.search_view)] Android.Support.V7.Widget.SearchView searchView;
        [InjectView(Resource.Id.loading_spinner)] Android.Widget.ProgressBar spinner;
#pragma warning restore 0649

        CategoriesAdapter Adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.lyt_search);
            Cheeseknife.Inject(this);

			searchView.QueryTextChange += (sender, e) =>
			{
				_timer.Change(1000, Timeout.Infinite);
			};

            categories.SetLayoutManager(new LinearLayoutManager(this));

            Adapter = new CategoriesAdapter(this);

            categories.SetAdapter(Adapter);

            Adapter.Click += OnClick;
			_timer = new Timer(onTimer);
            searchView.Iconified = false;
            searchView.ClearFocus();
            GetTags();
        }

        public void OnClick(int pos)
        {
            Intent returnIntent = new Intent();
            Bundle b = new Bundle();
            b.PutString("SEARCH", Adapter.GetItem(pos).Name);
            returnIntent.PutExtra("SEARCH", b);
            SetResult(Result.Ok, returnIntent);
            Finish();
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
				if (searchView?.Query?.Length != 1 && spinner != null)
				{
					spinner.Visibility = ViewStates.Visible;
					tagsTask = presenter.SearchCategories(searchView.Query);
					var tags = await tagsTask;
					if (tags?.Result?.Results != null)
					{
						Adapter.Reset(tags.Result.Results);
						Adapter.NotifyDataSetChanged();
					}
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				if(spinner != null)
					spinner.Visibility = ViewStates.Gone;
			}
        }

		protected override void CreatePresenter()
		{
			presenter = new SearchPresenter(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Cheeseknife.Reset(this);
		}
	}
}