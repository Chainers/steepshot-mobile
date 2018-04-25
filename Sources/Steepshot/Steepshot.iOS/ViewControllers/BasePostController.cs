using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BasePostController<T> : BaseViewControllerWithPresenter<T> where T : BasePostPresenter
    {
        private UIView popup;
        private UIView dialog;
        private UIButton rightButton;

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
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.DeletePost), UIAlertActionStyle.Default, obj => DeleteAlert(post)));
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

        private void DeleteAlert(Post post)
        {
            var titleText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertTitle);
            var messageText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertMessage);
            var leftButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
            var rightButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Delete);

            var commonMargin = 10;
            var dialogWidth = UIScreen.MainScreen.Bounds.Width - commonMargin * 2;

            popup = new UIView();
            popup.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
            popup.UserInteractionEnabled = true;

            dialog = new UIView();
            dialog.ClipsToBounds = true;
            dialog.Layer.CornerRadius = 15;
            dialog.BackgroundColor = UIColor.White;
            popup.AddSubview(dialog);

            dialog.AutoSetDimension(ALDimension.Width, dialogWidth);

            // Title

            var title = new UITextView();
            title.UserInteractionEnabled = false;
            title.Editable = false;
            title.Font = Constants.Regular20;
            title.TextAlignment = UITextAlignment.Center;
            title.Text = titleText;
            title.BackgroundColor = UIColor.Clear;
            dialog.AddSubview(title);

            title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 12);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);

            var size = title.SizeThatFits(new CGSize(dialogWidth - commonMargin * 2, 0));
            title.AutoSetDimension(ALDimension.Height, size.Height);

            // Alert message

            var message = new UITextView();
            message.UserInteractionEnabled = false;
            message.Editable = false;
            message.Font = Constants.Regular14;
            message.TextAlignment = UITextAlignment.Center;
            message.Text = messageText;
            message.BackgroundColor = UIColor.Clear;
            dialog.AddSubview(message);

            message.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 11);
            message.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            message.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);

            size = message.SizeThatFits(new CGSize(dialogWidth - commonMargin * 2, 0));
            message.AutoSetDimension(ALDimension.Height, size.Height);

            // Separator

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;
            dialog.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, message, 26);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            separator.AutoSetDimension(ALDimension.Height, 1);

            var leftButton = CreateButton(leftButtonText, UIColor.Black);
            leftButton.Font = Constants.Semibold14;
            leftButton.Layer.BorderWidth = 1;
            leftButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            dialog.AddSubview(leftButton);

            leftButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            leftButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            leftButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);
            leftButton.AutoSetDimension(ALDimension.Width, dialogWidth / 2 - 27);
            leftButton.AutoSetDimension(ALDimension.Height, 50);

            rightButton = CreateButton(rightButtonText, UIColor.White);
            rightButton.Font = Constants.Bold14;
            dialog.AddSubview(rightButton);

            rightButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            rightButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, leftButton, 15);
            rightButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            rightButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);
            rightButton.AutoSetDimension(ALDimension.Width, dialogWidth / 2 - 27);
            rightButton.AutoSetDimension(ALDimension.Height, 50);
            rightButton.LayoutIfNeeded();

            dialog.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 34);
            dialog.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);

            leftButton.TouchDown += (sender, e) => { HideDialog(); };
            rightButton.TouchDown += (sender, e) => { DeletePost(post); };

            NavigationController.View.EndEditing(true);
            TabBarController.View.AddSubview(popup);

            Constants.CreateGradient(rightButton, 25);
            Constants.CreateShadow(rightButton, Constants.R231G72B0, 0.5f, 25, 10, 12);

            var targetY = dialog.Frame.Y;
            dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, UIScreen.MainScreen.Bounds.Bottom);

            UIView.Animate(0.3, () =>
            {
                popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f);
                dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, targetY - 10);
            }, () =>
            {
                UIView.Animate(0.1, () =>
                {
                    dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, targetY);
                });
            });
        }

        private void HideDialog()
        {
            UIView.Animate(0.3, () =>
            {
                popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
                dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, UIScreen.MainScreen.Bounds.Bottom);
            }, () => popup.RemoveFromSuperview());
        }

        private async void DeletePost(Post post)
        {
            HideDialog();

            var error = await _presenter.TryDeletePost(post);

            if (error != null)
                ShowAlert(error);
        }

        public UIButton CreateButton(string title, UIColor titleColor)
        {
            var button = new UIButton();
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(titleColor, UIControlState.Normal);
            button.Layer.CornerRadius = 25;

            return button;
        }

        protected abstract void SameTabTapped();

        protected void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        protected async void ScrolledToBottom()
        {
            await GetPosts(false, false);
        }

        protected abstract Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false);

        protected void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }
    }
}
