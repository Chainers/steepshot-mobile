using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BasePostController<T> : BaseViewControllerWithPresenter<T> where T : BasePostPresenter, new()
    {
        protected async void Vote(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryVote(post);
            if (error is CanceledError)
                return;

            ShowAlert(error);
            if (error == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        protected void LoginTapped(object sender, EventArgs e)
        {
            var myViewController = new WelcomeViewController();
            NavigationController.PushViewController(myViewController, true);
        }

        protected void Flagged(Post post, List<UIAlertAction> actions = null)
        {
            var actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            if (actions != null)
                foreach (var action in actions)
                    actionSheetAlert.AddAction(action);
            if (post.Author == BasePresenter.User.Login)
            {
                //for edit and delete
                //actionSheetAlert.AddAction(UIAlertAction.Create("Edit post", UIAlertActionStyle.Default, null));
                //actionSheetAlert.AddAction(UIAlertAction.Create("Delete post", UIAlertActionStyle.Default, null));
            }
            else
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.FlagPhoto), UIAlertActionStyle.Default, obj => FlagPhoto(post)));
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.HidePhoto), UIAlertActionStyle.Default, obj => HidePhoto(post)));
            }
            //Sharepost contain copylink function by default
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Sharepost), UIAlertActionStyle.Default, obj => SharePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        protected void HidePhoto(Post post)
        {
            BasePresenter.User.PostBlackList.Add(post.Url);
            BasePresenter.User.Save();

            _presenter.HidePost(post);
        }

        protected async Task FlagPhoto(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
            if (error == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        private void CopyLink(Post post)
        {
            UIPasteboard.General.String = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostLink, post.Url);
            ShowAlert(LocalizationKeys.Copied);
        }

        private void SharePhoto(Post post)
        {
            var postLink = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostLink, post.Url);
            var item = NSObject.FromObject(postLink);
            var activityItems = new NSObject[] { item };

            var activityController = new UIActivityViewController(activityItems, null);
            PresentViewController(activityController, true, null);
        }

        protected abstract void SameTabTapped();
        protected abstract Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false);
        protected abstract void SourceChanged(Status status);

        protected async void ScrolledToBottom()
        {
            await GetPosts(false, false);
        }

        protected void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }

        protected override void CreatePresenter()
        {
            _presenter = new T();
            _presenter.SourceChanged += SourceChanged;
        }
    }
}
