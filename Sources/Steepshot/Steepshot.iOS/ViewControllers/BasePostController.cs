using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Views;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BasePostController<T> : BaseViewControllerWithPresenter<T> where T : BasePostPresenter, new()
    {
        private UIView dialog;
        private UIButton rightButton;
        private CustomAlertView _alert;

        protected async void Vote(Post post)
        {
            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped(null, null);
                return;
            }

            if (post == null)
                return;

            var exception = await _presenter.TryVoteAsync(post);
            if (exception is OperationCanceledException)
                return;

            ShowAlert(exception);
            if (exception == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        protected virtual async void LoginTapped(object sender, EventArgs e)
        {
            var response = await _presenter.CheckServiceStatusAsync();

            var myViewController = new WelcomeViewController(response.IsSuccess);
            NavigationController.PushViewController(myViewController, true);
        }

        protected void Flagged(Post post, List<UIAlertAction> actions = null)
        {
            var actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            if (actions != null)
                foreach (var action in actions)
                    actionSheetAlert.AddAction(action);
            if (post.Author == AppSettings.User.Login)
            {
                if (post.CashoutTime > post.Created)
                {
                    actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.EditPost), UIAlertActionStyle.Default, obj => EditPost(post)));

                    actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.DeletePost), UIAlertActionStyle.Default, obj => DeleteAlert(post)));
                }
            }
            else
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.FlagPhoto), UIAlertActionStyle.Default, obj => FlagPhoto(post)));
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.HidePhoto), UIAlertActionStyle.Default, obj => HidePhoto(post)));
            }
            actionSheetAlert.AddAction(UIAlertAction.Create("Promote", UIAlertActionStyle.Default, obj => ShowPromotePopup(post)));
            //Sharepost contain copylink function by default
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Sharepost), UIAlertActionStyle.Default, obj => SharePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        public override void ViewDidAppear(bool animated)
        {
            if(_alert != null)
                _alert.Hidden = false;

            base.ViewDidAppear(animated);
        }

        private void ShowPromotePopup(Post post)
        {
            var promotePopup = new Popups.PromotePopup();
            _alert = promotePopup.Create(post, TabBarController != null ? TabBarController.NavigationController : NavigationController, View);
        }

        protected void HidePhoto(Post post)
        {
            AppSettings.User.PostBlackList.Add(post.Url);
            AppSettings.User.Save();

            _presenter.HidePost(post);
        }

        protected async Task FlagPhoto(Post post)
        {
            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped(null, null);
                return;
            }

            if (post == null)
                return;

            var exception = await _presenter.TryFlagAsync(post);
            ShowAlert(exception);
            if (exception == null)
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
            CustomAlertView _deleteAlert = null;

            if (_deleteAlert == null)
            {
                var titleText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertTitle);
                var messageText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertMessage);
                var leftButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
                var rightButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Delete);

                var commonMargin = 20;
                var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;

                dialog = new UIView();
                dialog.ClipsToBounds = true;
                dialog.Layer.CornerRadius = 15;
                dialog.BackgroundColor = UIColor.White;

                dialog.AutoSetDimension(ALDimension.Width, dialogWidth);

                // Title

                var title = new UILabel();
                title.Lines = 3;
                title.LineBreakMode = UILineBreakMode.WordWrap;
                title.UserInteractionEnabled = false;
                title.Font = Constants.Regular20;
                title.TextAlignment = UITextAlignment.Center;
                title.Text = titleText;
                title.BackgroundColor = UIColor.Clear;
                dialog.AddSubview(title);

                title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 24);
                title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 10);
                title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);

                var size = title.SizeThatFits(new CGSize(dialogWidth - commonMargin * 2, 0));
                title.AutoSetDimension(ALDimension.Height, size.Height);

                // Alert message

                var message = new UILabel();
                message.Lines = 9;
                message.LineBreakMode = UILineBreakMode.WordWrap;
                message.UserInteractionEnabled = false;
                message.Font = Constants.Regular14;
                message.TextAlignment = UITextAlignment.Center;
                message.Text = messageText;
                message.BackgroundColor = UIColor.Clear;
                dialog.AddSubview(message);

                message.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 22);
                message.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 10);
                message.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);

                size = message.SizeThatFits(new CGSize(dialogWidth - commonMargin * 2, 0));
                message.AutoSetDimension(ALDimension.Height, size.Height);

                // Separator

                var separator = new UIView();
                separator.BackgroundColor = Constants.R245G245B245;
                dialog.AddSubview(separator);

                separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, message, 26);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                separator.AutoSetDimension(ALDimension.Height, 1);

                var leftButton = CreateButton(leftButtonText, UIColor.Black);
                leftButton.Font = Constants.Semibold14;
                leftButton.Layer.BorderWidth = 1;
                leftButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
                dialog.AddSubview(leftButton);

                leftButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
                leftButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                leftButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
                leftButton.AutoSetDimension(ALDimension.Width, dialogWidth / 2 - 27);
                leftButton.AutoSetDimension(ALDimension.Height, 50);

                rightButton = CreateButton(rightButtonText, UIColor.White);
                rightButton.Font = Constants.Bold14;
                dialog.AddSubview(rightButton);

                rightButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
                rightButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, leftButton, 15);
                rightButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                rightButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
                rightButton.AutoSetDimension(ALDimension.Width, dialogWidth / 2 - 27);
                rightButton.AutoSetDimension(ALDimension.Height, 50);
                rightButton.LayoutIfNeeded();

                NavigationController.View.EndEditing(true);

                _deleteAlert = new CustomAlertView(dialog, TabBarController);

                leftButton.TouchDown += (sender, e) => { _deleteAlert.Close(); };
                rightButton.TouchDown += (sender, e) => { DeletePost(post, _deleteAlert.Close); };

                Constants.CreateGradient(rightButton, 25);
                Constants.CreateShadow(rightButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
            }
            _deleteAlert.Show();
        }

        private async void DeletePost(Post post, Action action)
        {
            action.Invoke();

            var exception = await _presenter.TryDeletePostAsync(post);

            if (exception != null)
                ShowAlert(exception);
        }

        private void EditPost(Post post)
        {
            var editPostViewController = new PostEditViewController(post);
            TabBarController.NavigationController.PushViewController(editPostViewController, true);
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
        protected abstract Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false);
        protected abstract void SourceChanged(Status status);

        protected async void ScrolledToBottom()
        {
            await GetPosts(false, false);
        }
    }
}
