using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
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
    public class SearchFragment : BaseFragmentWithPresenter<SearchPresenter>
    {
        private Timer _timer;
        private SearchType _searchType = SearchType.People;
        private Typeface _font;
        private Typeface _semiboldFont;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.categories)] RecyclerView _categories;
        [InjectView(Resource.Id.users)] RecyclerView _users;
        [InjectView(Resource.Id.search_view)] EditText _searchView;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar _spinner;
        [InjectView(Resource.Id.tags_button)] Button _tagsButton;
        [InjectView(Resource.Id.people_button)] Button _peopleButton;
        [InjectView(Resource.Id.clear_button)] Button _clearButton;
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
            _searchView.TextChanged += (sender, e) =>
            {
                _timer.Change(500, Timeout.Infinite);
            };

            _categories.SetLayoutManager(new LinearLayoutManager(Activity));
            _users.SetLayoutManager(new LinearLayoutManager(Activity));

            _categoriesAdapter = new CategoriesAdapter();
            _categoriesAdapter.Items = _presenter.Tags;
            _usersSearchAdapter = new UsersSearchAdapter(Activity);
            _usersSearchAdapter.Items = _presenter.Users;
            _categories.SetAdapter(_categoriesAdapter);
            _users.SetAdapter(_usersSearchAdapter);

            _categoriesAdapter.Click += OnClick;
            _usersSearchAdapter.Click += OnClick;
            _timer = new Timer(OnTimer);
            _font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            _semiboldFont = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");

            _searchView.Typeface = _font;
            _clearButton.Typeface = _font;
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
            else if (_searchType == SearchType.People)
            {
                if (_usersSearchAdapter.Items.Count > pos)
                {
                    var user = _usersSearchAdapter.Items[pos].Username;
                    ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(user));
                }
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
                _spinner.Visibility = ViewStates.Visible;

                var errors = await _presenter.SearchCategories(_searchView.Text, _searchType);
                if (errors != null && errors.Count > 0)
                    Toast.MakeText(Activity, errors[0], ToastLength.Short).Show();
                else
                {
                    if (_searchType == SearchType.Tags)
                        _categoriesAdapter.NotifyDataSetChanged();
                    else
                        _usersSearchAdapter.NotifyDataSetChanged();
                }

                if (_spinner != null)
                    _spinner.Visibility = ViewStates.Gone;
            }
            catch(Exception ex)
            {
                
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
                _tagsButton.Typeface = _semiboldFont;
                _tagsButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
                _tagsButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30)));

                _peopleButton.Typeface = _font;
                _peopleButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                _peopleButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));
            }
            else
            {
                _users.Visibility = ViewStates.Visible;
                _categories.Visibility = ViewStates.Gone;
                _peopleButton.Typeface = _semiboldFont;
                _peopleButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
                _peopleButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30)));

                _tagsButton.Typeface = _font;
                _tagsButton.SetTextSize(Android.Util.ComplexUnitType.Sp, 14);
                _tagsButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));
            }
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }
    }
}
