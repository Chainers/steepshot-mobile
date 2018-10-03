using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;

namespace Steepshot.Fragment
{
    public sealed class VotersFragment : BaseFragmentWithPresenter<UserFriendPresenter>
    {
        public static string VotersType = "voterstype";
        private FollowersAdapter _adapter;
        private string _url;
        private bool _isLikers;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [BindView(Resource.Id.followers_list)] private RecyclerView _votersList;
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.people_count)] private TextView _peopleCount;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_followers, null);
                Cheeseknife.Bind(this, InflatedView);
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
            _peopleCount.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PeopleText, count);

            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBackClick;
            _switcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;
            _viewTitle.Typeface = Style.Semibold;
            _peopleCount.Typeface = Style.Regular;

            _isLikers = Activity.Intent.GetBooleanExtra(VotersType, true);
            _viewTitle.Text = AppSettings.LocalizationManager.GetText(_isLikers ? LocalizationKeys.Voters : LocalizationKeys.FlagVoters);

            _url = Activity.Intent.GetStringExtra(FeedFragment.PostUrlExtraPath);
            Presenter.SourceChanged += PresenterSourceChanged;
            Presenter.VotersType =
                _isLikers ? Core.Models.Enums.VotersType.Likes : Core.Models.Enums.VotersType.Flags;
            _adapter = new FollowersAdapter(Activity, Presenter);
            _adapter.UserAction += OnClick;
            _adapter.FollowAction += OnFollow;
            _votersList.SetAdapter(_adapter);
            var scrollListner = new ScrollListener();
            scrollListner.ScrolledToBottom += LoadNext;
            _votersList.AddOnScrollListener(scrollListner);
            _votersList.SetLayoutManager(new LinearLayoutManager(Activity));

            _emptyQueryLabel.Typeface = Style.Light;
            _emptyQueryLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EmptyCategory);

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
            var exception = await Presenter.TryLoadNextPostVotersAsync(_url);
            if (!IsInitialized)
                return;

            Context.ShowAlert(exception, ToastLength.Short);
            _bar.Visibility = ViewStates.Gone;

            _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        private void OnClick(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            if (userFriend.Author == AppSettings.User.Login)
                return;

            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(userFriend.Author));
        }

        private async void OnFollow(UserFriend userFriend)
        {
            if (userFriend == null)
                return;

            if (AppSettings.User.HasPostingPermission)
            {
                var result = await Presenter.TryFollowAsync(userFriend);
                if (!IsInitialized)
                    return;

                Context.ShowAlert(result, ToastLength.Short);
            }
            else
            {
                var intent = new Intent(Activity, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            _adapter.NotifyDataSetChanged();
        }
    }
}
