using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;

namespace Steepshot
{
	[Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class VotersActivity : BaseActivity, FollowersView
	{
		private VotersPresenter presenter;
		private VotersAdapter _votersAdapter;

#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.loading_spinner)]
		private ProgressBar _bar;
		[InjectView(Resource.Id.followers_list)]
		private RecyclerView _votersList;
		[InjectView(Resource.Id.Title)]
		private TextView _viewTitle;
		[InjectView(Resource.Id.btn_back)]
		private ImageButton _backButton;
#pragma warning restore 0649

		protected override void CreatePresenter()
		{
			presenter = new VotersPresenter(this);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.lyt_followers);
			Cheeseknife.Inject(this);
			_backButton.Visibility = ViewStates.Gone;
			_viewTitle.Text = "Voters";
			var url = Intent.GetStringExtra("url");
			_votersAdapter = new VotersAdapter(this, presenter.Users);
			_votersAdapter.Click += OnClick;
			_votersList.SetAdapter(_votersAdapter);
			_votersList.AddOnScrollListener(new VotersScrollListener(presenter, url));
			_votersList.SetLayoutManager(new LinearLayoutManager(this));
			presenter.VotersLoaded += OnPostLoaded;
			presenter.GetItems(url);
		}

		private void OnPostLoaded()
		{
			RunOnUiThread(() =>
				{
					if (_bar != null)
						_bar.Visibility = ViewStates.Gone;
					_votersAdapter?.NotifyDataSetChanged();
				});
		}

		private void OnClick(int pos)
		{
			Intent intent = new Intent(this, typeof(ProfileActivity));
			intent.PutExtra("ID", _votersAdapter.Items[pos].Username);
			this.StartActivity(intent);
		}
	}

	public class VotersScrollListener : RecyclerView.OnScrollListener
	{
		VotersPresenter presenter;
		private string _url;

		public VotersScrollListener(VotersPresenter presenter, string url)
		{
			this.presenter = presenter;
			_url = url;
		}
		int prevPos = 0;
		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
			if (pos > prevPos && pos != prevPos)
			{
				if (pos == recyclerView.GetAdapter().ItemCount - 1)
				{
					if (pos < ((VotersAdapter)recyclerView.GetAdapter()).ItemCount)
					{
						Task.Run(() => presenter.GetItems(_url));
						prevPos = pos;
					}
				}
			}
		}

		public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
		{

		}
	}
}
