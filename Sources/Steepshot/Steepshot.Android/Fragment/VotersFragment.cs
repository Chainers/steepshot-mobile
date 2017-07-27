using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Presenter;

using Steepshot.View;

namespace Steepshot.Fragment
{
	public class VotersFragment : BaseFragment, IFollowersView
	{
		private VotersPresenter _presenter;
		private VotersAdapter _votersAdapter;

#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
		[InjectView(Resource.Id.followers_list)] private RecyclerView _votersList;
		[InjectView(Resource.Id.Title)] private TextView _viewTitle;
		[InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
#pragma warning restore 0649

		protected override void CreatePresenter()
		{
			_presenter = new VotersPresenter(this);
		}

		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!IsInitialized)
			{
				V = inflater.Inflate(Resource.Layout.lyt_followers, null);
				Cheeseknife.Inject(this, V);
			}
			return V;
		}

		public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
		{
			if (IsInitialized)
				return;
			 base.OnViewCreated(view, savedInstanceState);
			_viewTitle.Text = "Voters";
			var url = Activity.Intent.GetStringExtra("url");
			_votersAdapter = new VotersAdapter(Activity, _presenter.Users);
			_votersAdapter.Click += OnClick;
			_votersList.SetAdapter(_votersAdapter);
			_votersList.AddOnScrollListener(new VotersScrollListener(_presenter, url));
			_votersList.SetLayoutManager(new LinearLayoutManager(Activity));
			_presenter.VotersLoaded += OnPostLoaded;
			_presenter.GetItems(url);
		}

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
		{
			Activity.OnBackPressed();
		}

		private void OnPostLoaded()
		{
			Activity.RunOnUiThread(() =>
				{
					if (_bar != null)
						_bar.Visibility = ViewStates.Gone;
					_votersAdapter?.NotifyDataSetChanged();
				});
		}

		private void OnClick(int pos)
		{
			((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_votersAdapter.Items[pos].Username));
		}

		public override void OnDetach()
		{
			base.OnDetach();
			Cheeseknife.Reset(this);
		}
	}

	public class VotersScrollListener : RecyclerView.OnScrollListener
	{
		VotersPresenter _presenter;
		private string _url;

		public VotersScrollListener(VotersPresenter presenter, string url)
		{
			this._presenter = presenter;
			_url = url;
		}
		int _prevPos = 0;
		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
			if (pos > _prevPos && pos != _prevPos)
			{
				if (pos == recyclerView.GetAdapter().ItemCount - 1)
				{
					if (pos < ((VotersAdapter)recyclerView.GetAdapter()).ItemCount)
					{
						Task.Run(() => _presenter.GetItems(_url));
						_prevPos = pos;
					}
				}
			}
		}

		public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
		{

		}
	}
}
