using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class VotersFragment : BaseFragmentWithPresenter<VotersPresenter>
    {
        private VotersAdapter _votersAdapter;
        private string _url;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.followers_list)] private RecyclerView _votersList;
        [InjectView(Resource.Id.Title)] private TextView _viewTitle;
#pragma warning restore 0649

        protected override void CreatePresenter()
        {
            _presenter = new VotersPresenter();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                V = inflater.Inflate(Resource.Layout.lyt_followers, null);
                Cheeseknife.Inject(this, V);
            }
            return V;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;
            base.OnViewCreated(view, savedInstanceState);
            //_viewTitle.Text = Localization.Messages.Voters;
            _url = Activity.Intent.GetStringExtra("url");
            _votersAdapter = new VotersAdapter(Activity, _presenter.Voters);
            _votersAdapter.Click += OnClick;
            _votersList.SetAdapter(_votersAdapter);
            var scrollListner = new ScrollListener();
            scrollListner.ScrolledToBottom += LoadNext;
            _votersList.AddOnScrollListener(scrollListner);
            _votersList.SetLayoutManager(new LinearLayoutManager(Activity));
            LoadNext();
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        public override void OnDestroy()
        {
            _presenter.LoadCancel();
            base.OnDestroy();
        }

        private async void LoadNext()
        {
            var errors = await _presenter.TryLoadNext(_url);

            if (errors != null && errors.Count > 0)
                ShowAlert(errors);
            else
                _votersAdapter?.NotifyDataSetChanged();

            if (_bar != null)
                _bar.Visibility = ViewStates.Gone;
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
}
