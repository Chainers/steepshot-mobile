using System;
using System.Collections.Generic;
using System.Linq;
using FFImageLoading;
using Foundation;
using PureLayout.Net;
using SafariServices;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class PlagiarismViewController : DescriptionViewController, ISFSafariViewControllerDelegate
    {
        private UIButton cancelButton;
        private UIButton continueButton;

        public PlagiarismViewController(List<Tuple<NSDictionary, UIImage>> assets)
        {
            ImageAssets = assets;
            plagiarismMode = true;
        }

        public override void ViewDidLoad()
        {
            SetupMainScroll();
            CreateView();
            SetNavigationBar();
        }

        public override void ViewDidLayoutSubviews()
        {
            if (!_isinitialized)
            {
                cancelButton.LayoutIfNeeded();
                Constants.CreateGradient(cancelButton, 25);
                Constants.CreateShadow(cancelButton, Constants.R231G72B0, 0.5f, 25, 10, 12);

                continueButton.LayoutIfNeeded();
                Constants.CreateGradient(continueButton, 25, GradientType.Blue);
                Constants.CreateShadow(continueButton, Constants.R26G151B246, 0.5f, 25, 10, 12);

                _isinitialized = true;
            }
        }

        private void SetupMainScroll()
        {
            mainScroll = CreateScrollView();

            View.AddSubview(mainScroll);

            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
        }

        private void SetNavigationBar()
        {
            var leftBarButton = new UIButton();
            leftBarButton.SetImage(UIImage.FromBundle("ic_back_arrow"), UIControlState.Normal);
            leftBarButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PlagiarismTitle), UIControlState.Normal);
            leftBarButton.ImageEdgeInsets = new UIEdgeInsets(0, 0, 0, 20);
            leftBarButton.TitleEdgeInsets = new UIEdgeInsets(0, 20, 0, -20);
            leftBarButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            leftBarButton.TitleLabel.Font = Constants.Semibold16;
            leftBarButton.TitleLabel.TextColor = Constants.R15G24B30;
            leftBarButton.AddTarget(GoBack, UIControlEvent.TouchDown);
            leftBarButton.SizeToFit();

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(leftBarButton);

            var guidelines = new UIBarButtonItem(AppSettings.LocalizationManager.GetText(LocalizationKeys.GuidelinesForPlagiarism), UIBarButtonItemStyle.Plain, OpenGuidelines);
            var textAttributes = new UITextAttributes();
            textAttributes.Font = Constants.Semibold14;
            textAttributes.TextColor = Constants.R255G34B5;
            guidelines.SetTitleTextAttributes(textAttributes, UIControlState.Normal);
            guidelines.SetTitleTextAttributes(textAttributes, UIControlState.Selected);
            NavigationItem.RightBarButtonItem = guidelines;

            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;
            NavigationController.NavigationBar.Translucent = false;
        }

        private void CreateView()
        {
            GetPostSize();
            SetImage();

            var photoTitleSeparator = new UIView();
            photoTitleSeparator.BackgroundColor = Constants.R245G245B245;

            var plagiarismDescription = new UILabel();
            plagiarismDescription.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PhotoPlagiarism);
            plagiarismDescription.Font = Constants.Semibold20;
            plagiarismDescription.TextColor = Constants.R15G24B30;
            plagiarismDescription.UserInteractionEnabled = false;
            plagiarismDescription.Lines = 0;

            cancelButton = new UIButton();
            cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.CancelPublishing).ToUpper(), UIControlState.Normal);
            cancelButton.Layer.CornerRadius = 25;
            cancelButton.TitleLabel.Font = Constants.Bold14;
            cancelButton.TitleLabel.TextColor = Constants.R255G255B255;

            continueButton = new UIButton();
            continueButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.ContinuePublishing).ToUpper(), UIControlState.Normal);
            continueButton.Layer.CornerRadius = 25;
            continueButton.TitleLabel.Font = Constants.Bold14;
            continueButton.TitleLabel.TextColor = Constants.R255G255B255;

            mainScroll.AddSubview(photoTitleSeparator);
            mainScroll.AddSubview(plagiarismDescription);
            mainScroll.AddSubview(cancelButton);
            mainScroll.AddSubview(continueButton);

            if (photoView != null)
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoView, 24f);
            else
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoCollection, 24f);

            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            photoTitleSeparator.AutoSetDimension(ALDimension.Height, 1f);
            photoTitleSeparator.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width - _separatorMargin * 2);

            plagiarismDescription.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            plagiarismDescription.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            plagiarismDescription.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoTitleSeparator, 24);

            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, plagiarismDescription, 30);
            cancelButton.AutoSetDimension(ALDimension.Height, 50);

            continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 34);
            continueButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, cancelButton, 10);
            continueButton.AutoSetDimension(ALDimension.Height, 50);
        }

        private void OpenGuidelines(object sender, EventArgs e)
        { 
            var sv = new SFSafariViewController(new Uri(Core.Constants.Guide));
            sv.Delegate = this;

            NavigationController.SetNavigationBarHidden(true, false);
            NavigationController.PushViewController(sv, false);
        }

        [Export("safariViewControllerDidFinish:")]
        public void DidFinish(SFSafariViewController controller)
        {
            NavigationController.SetNavigationBarHidden(false, false);
            NavigationController.PopViewController(false);
        }
    }
}
