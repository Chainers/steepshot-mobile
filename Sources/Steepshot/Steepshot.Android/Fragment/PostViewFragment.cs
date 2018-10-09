using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Autofac;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostViewFragment : BaseFragmentWithPresenter<SinglePostPresenter>
    {
#pragma warning disable 0649, 4014
        [BindView(Resource.Id.right_btns_layout)] private LinearLayout _rightButtons;
        [BindView(Resource.Id.single_post)] private FrameLayout _container;
        [BindView(Resource.Id.close)] private ImageButton _closeButton;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _loadingBar;
        [BindView(Resource.Id.profile_login)] private TextView _header;
#pragma warning restore 0649

        private PostViewHolder _postViewHolder;
        private PostViewHolder PostViewHolder => _postViewHolder ?? (_postViewHolder = new PostViewHolder(InflatedView, PostAction, AutoLinkAction, null, Style.ScreenWidth, Style.ScreenWidth));
        private readonly string _url;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_single_post, null);
                Cheeseknife.Bind(this, InflatedView);
            }
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            ToggleTabBar();

            _container.Visibility = ViewStates.Gone;
            _loadingBar.Visibility = ViewStates.Visible;

            _closeButton.Click += BackButtonOnClick;

            if (!string.IsNullOrEmpty(_url))
            {
                Presenter.SourceChanged += PresenterOnSourceChanged;
                Presenter.TryLoadPostInfoAsync(_url);
            }
        }
        
        private void BackButtonOnClick(object sender, EventArgs eventArgs) => ((BaseActivity)Activity).OnBackPressed();

        private void PresenterOnSourceChanged(Status status)
        {
            _container.Visibility = ViewStates.Visible;
            _loadingBar.Visibility = ViewStates.Gone;
            PostViewHolder.UpdateData(Presenter.PostInfo, Activity);
        }

        public PostViewFragment(string url)
        {
            _url = url;
        }

        private async void PostAction(ActionType type, Post post)
        {
            if (post == null)
                return;

            switch (type)
            {
                case ActionType.Like:
                    {
                        if (!App.User.HasPostingPermission)
                            return;

                        var result = await Presenter.TryVoteAsync(post);
                        if (!IsInitialized)
                            return;

                        if (result.IsSuccess && Activity is RootActivity root)
                        {
                            root.TryUpdateProfile();
                            PostViewHolder.UpdateData(post, Activity);
                        }

                        Context.ShowAlert(result);
                        break;
                    }
                case ActionType.VotersLikes:
                case ActionType.VotersFlags:
                    {
                        var isLikers = type == ActionType.VotersLikes;
                        Activity.Intent.PutExtra(FeedFragment.PostUrlExtraPath, post.Url);
                        Activity.Intent.PutExtra(FeedFragment.PostNetVotesExtraPath, isLikers ? post.NetLikes : post.NetFlags);
                        Activity.Intent.PutExtra(VotersFragment.VotersType, isLikers);
                        ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
                        break;
                    }
                case ActionType.Comments:
                    {
                        ((BaseActivity)Activity).OpenNewContentFragment(new CommentsFragment(post, post.Children == 0));
                        break;
                    }
                case ActionType.Profile:
                    {
                        ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
                        break;
                    }
                case ActionType.Flag:
                    {
                        if (!App.User.HasPostingPermission)
                            return;
                        
                        var result = await Presenter.TryFlagAsync(post);
                        if (!IsInitialized)
                            return;

                        if (result.IsSuccess && Activity is RootActivity root)
                        {
                            root.TryUpdateProfile();
                            PostViewHolder.UpdateData(post, Activity);
                        }

                        Context.ShowAlert(result);
                        break;
                    }
                case ActionType.Delete:
                    {
                        var result = await Presenter.TryDeletePostAsync(post);
                        if (!IsInitialized)
                            return;

                        if (result.IsSuccess)
                            ((BaseActivity)Activity).OnBackPressed();

                        Context.ShowAlert(result);
                        break;
                    }
                case ActionType.Edit:
                    {
                        ((BaseActivity)Activity).OpenNewContentFragment(new PostEditFragment(post));
                        ToggleTabBar(true);
                        break;
                    }
                case ActionType.Share:
                    {
                        var shareIntent = new Intent(Intent.ActionSend);
                        shareIntent.SetType("text/plain");
                        shareIntent.PutExtra(Intent.ExtraSubject, post.Title);
                        shareIntent.PutExtra(Intent.ExtraText, string.Format(App.User.Chain == KnownChains.Steem ? Constants.SteemPostUrl : Constants.GolosPostUrl, post.Url));
                        StartActivity(Intent.CreateChooser(shareIntent, App.Localization.GetText(LocalizationKeys.Sharepost)));
                        break;
                    }
                case ActionType.Photo:
                case ActionType.Preview:
                    {
                        var intent = new Intent(Context, typeof(PostPreviewActivity));
                        intent.PutExtra(PostPreviewActivity.PhotoExtraPath, post.Media[0].Url);
                        StartActivity(intent);
                        break;
                    }
                case ActionType.Promote:
                    {
                        var actionAlert = new PromoteAlertDialog(Context, post, AutoLinkAction);
                        actionAlert.Window.RequestFeature(WindowFeatures.NoTitle);
                        actionAlert.Show();
                        break;
                    }
            }
        }
    }
}