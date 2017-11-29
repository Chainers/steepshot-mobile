using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;

namespace Steepshot.Fragment
{
    public sealed class VotersFragment : BaseFragmentWithPresenter<UserFriendPresenter>
    {
        public static string VotersType = "voterstype";
        private FollowersAdapter _adapter;
        private string _url;
        private bool _isLikers;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [InjectView(Resource.Id.followers_list)] private RecyclerView _votersList;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.people_count)] private TextView _peopleCount;
        [InjectView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_followers, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            ToggleTabBar();
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            var count = Activity.Intent.GetIntExtra(FeedFragment.PostNetVotesExtraPath, 0);
            _peopleCount.Text = $"{count:N0} {Localization.Texts.PeopleText}";

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBackClick;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Typeface = Style.Semibold;
            _peopleCount.Typeface = Style.Regular;

            _isLikers = Activity.Intent.GetBooleanExtra(VotersType, true);
            _viewTitle.Text = _isLikers ? Localization.Messages.Voters : Localization.Messages.FlagVoters;

            _url = Activity.Intent.GetStringExtra(FeedFragment.PostUrlExtraPath);
            Presenter.SourceChanged += PresenterSourceChanged;
            Presenter.VotersType =
                _isLikers ? Core.Models.Requests.VotersType.Likes : Core.Models.Requests.VotersType.Flags;
            _adapter = new FollowersAdapter(Activity, Presenter);
            _adapter.UserAction += OnClick;
            _adapter.FollowAction += OnFollow;
            _votersList.SetAdapter(_adapter);
            var scrollListner = new ScrollListener();
            scrollListner.ScrolledToBottom += LoadNext;
            _votersList.AddOnScrollListener(scrollListner);
            _votersList.SetLayoutManager(new LinearLayoutManager(Activity));

            _emptyQueryLabel.Typeface = Style.Light;
            _emptyQueryLabel.Text = Localization.Texts.EmptyQuery;

            LoadNext();
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }


        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _adapter.NotifyDataSetChanged();
            });
        }

        private void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        public override void OnDestroy()
        {
            Presenter.LoadCancel();
            base.OnDestroy();
        }

        private async void LoadNext()
        {
            var errors = await Presenter.TryLoadNextPostVoters(_url);
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors);
            _bar.Visibility = ViewStates.Gone;

            _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        private void OnClick(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            if (userFriend.Author == BasePresenter.User.Login)
                return;

            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(userFriend.Author));
        }

        private async void OnFollow(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            var errors = await Presenter.TryFollow(userFriend);
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors, ToastLength.Short);
        }
    }
}
