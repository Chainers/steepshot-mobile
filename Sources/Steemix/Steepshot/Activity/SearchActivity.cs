using System;
using Android.Content.PM;
using Android.App;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Android.Support.V7.Widget;
using Android.Views.InputMethods;
using Android.Views;
using Android.Content;

namespace Steepshot
{
    [Activity(Label = "SearchActivity", ScreenOrientation = ScreenOrientation.Portrait)]
	public class SearchActivity : BaseActivity,SearchView
    {
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

            searchView.QueryTextChange += SearchView_QueryTextChange;

            categories.SetLayoutManager(new LinearLayoutManager(this));

            Adapter = new CategoriesAdapter(this);

            categories.SetAdapter(Adapter);

            Adapter.Click += OnClick;

            searchView.Iconified = false;
            searchView.ClearFocus();
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

        private async void SearchView_QueryTextChange(object sender,Android.Support.V7.Widget.SearchView.QueryTextChangeEventArgs e)
        {
            if (e.NewText.Length > 2) {
                spinner.Visibility = Android.Views.ViewStates.Visible;
				var cats = await presenter.SearchCategories(e.NewText);
                Adapter.Reset(cats.Result.Results);
                Adapter.NotifyDataSetChanged();
                spinner.Visibility = Android.Views.ViewStates.Gone;
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