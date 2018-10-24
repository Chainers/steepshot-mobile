using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public abstract class BasePostsFragment<T> : BaseFragmentWithPresenter<T>
        where T : BasePostPresenter
    {
        protected virtual int PositionOffset => 0;
#pragma warning disable 0649, 4014
        [BindView(Resource.Id.posts_list)] protected RecyclerView PostsList;
        [BindView(Resource.Id.refresher)] protected SwipeRefreshLayout Refresher;
#pragma warning restore 0649

        protected string ProfileId = App.User.Login;

        protected async void ScrollListnerScrolledToBottom()
        {
            await GetPosts(false);
        }

        protected abstract Task GetPosts(bool isRefresh);

        protected void OpenLogin()
        {
            var intent = new Intent(Activity, typeof(WelcomeActivity));
            StartActivity(intent);
        }

        protected async void PostAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Like:
                    {
                        if (App.User.HasPostingPermission)
                        {
                            var result = await Presenter.TryVoteAsync(post);
                            if (!IsInitialized)
                                return;

                            if (result.IsSuccess && Activity is RootActivity root)
                                root.TryUpdateProfile();

                            Context.ShowAlert(result);
                        }
                        else
                        {
                            OpenLogin();
                        }
                        break;
                    }
                case ActionType.VotersLikes:
                case ActionType.VotersFlags:
                    {
                        var isLikers = type == ActionType.VotersLikes;
                        Activity.Intent.PutExtra(FeedFragment.PostUrlExtraPath, post.Url);
                        Activity.Intent.PutExtra(FeedFragment.PostNetVotesExtraPath, isLikers ? post.NetLikes : post.NetFlags);
                        Activity.Intent.PutExtra(VotersFragment.VotersType, isLikers);
                        //TODO:KOA:Use constructor instead PutExtra
                        ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
                        break;
                    }
                case ActionType.Comments:
                    {
                        if (post.Children == 0 && !App.User.HasPostingPermission)
                        {
                            OpenLogin();
                            return;
                        }

                        ((BaseActivity)Activity).OpenNewContentFragment(new CommentsFragment(post, post.Children == 0));
                        break;
                    }
                case ActionType.Profile:
                    {
                        if (ProfileId != post.Author)
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
                            root.TryUpdateProfile();

                        Context.ShowAlert(result);
                        break;
                    }
                case ActionType.Hide:
                    {
                        Presenter.HidePost(post);
                        break;
                    }
                case ActionType.Edit:
                    {
                        ((BaseActivity)Activity).OpenNewContentFragment(new PostEditFragment(post));
                        ToggleTabBar(true);
                        break;
                    }
                case ActionType.Delete:
                    {
                        var actionAlert = new ActionAlertDialog(Context,
                            App.Localization.GetText(LocalizationKeys.DeleteAlertTitle),
                            App.Localization.GetText(LocalizationKeys.DeleteAlertMessage),
                            App.Localization.GetText(LocalizationKeys.Delete),
                            App.Localization.GetText(LocalizationKeys.Cancel), AutoLinkAction);

                        actionAlert.AlertAction += async () =>
                        {
                            var result = await Presenter.TryDeletePostAsync(post);
                            if (!IsInitialized)
                                return;
                            Context.ShowAlert(result);
                        };

                        actionAlert.Show();
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
                    {
                        OpenPost(post);
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

        protected void OpenPost(Post post)
        {
            var pagerFragment = new PostPagerFragment(Presenter, post, PostAction, GetPosts);
            pagerFragment.Close += Close;
            ((BaseActivity)Activity).OpenNewContentFragment(pagerFragment);
        }

        private void Close(int currentPost)
        {
            var lytManager = PostsList.GetLayoutManager();
            if (lytManager is GridLayoutManager gridLayoutManager)
            {
                var positionToScroll = currentPost + (currentPost - gridLayoutManager.FindFirstVisibleItemPosition()) / 2;
                PostsList.Post(() =>
                gridLayoutManager.ScrollToPosition(positionToScroll < Presenter.Count
                    ? positionToScroll
                    : Presenter.Count));
            }
            else if (lytManager is LinearLayoutManager linearLayoutManager)
            {
                PostsList.Post(() =>
                    linearLayoutManager.ScrollToPosition(currentPost + PositionOffset));
            }
        }

        protected void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        public override void OnDetach()
        {
            PostsList.SetAdapter(null);
            base.OnDetach();
        }
    }
}