using System;
using System.Collections.Generic;
using Foundation;
using PureLayout.Net;
using SafariServices;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class PlagiarismViewController : DescriptionViewController, ISFSafariViewControllerDelegate
    {
        private readonly Plagiarism _model;
        private Xamarin.TTTAttributedLabel.TTTAttributedLabel _plagiarismAttributedLabel;
        private UIButton _cancelButton;
        private UIButton _continueButton;
        private PlagiarismResult plagiarismResult;

        public PlagiarismViewController(List<Tuple<NSDictionary, UIImage>> assets, Plagiarism model, PlagiarismResult plagiarismResult = null)
        {
            ImageAssets = assets;
            _model = model;
            plagiarismMode = true;
            this.plagiarismResult = plagiarismResult;
        }

        public override void ViewDidLoad()
        {
            SetupMainScroll();
            SetNavigationBar();
            CreateView();
        }

        public override void ViewDidLayoutSubviews()
        {
            if (!_isinitialized)
            {
                _cancelButton.LayoutIfNeeded();
                Constants.CreateGradient(_cancelButton, 25);
                Constants.CreateShadow(_cancelButton, Constants.R231G72B0, 0.5f, 25, 10, 12);

                _continueButton.LayoutIfNeeded();
                Constants.CreateGradient(_continueButton, 25, GradientType.Blue);
                Constants.CreateShadow(_continueButton, Constants.R26G151B246, 0.5f, 25, 10, 12);

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

            var noLinkTitleAttribute = new UIStringAttributes
            { 
                Font = Constants.Semibold20,
                ForegroundColor = Constants.R15G24B30,
            };

            var similarAttribute = new UIStringAttributes
            {
                Link = new NSUrl(PlagiarismLinkType.Similar.ToString()),
                Font = Constants.Semibold20,
                ForegroundColor = Constants.R255G34B5,
            };

            var authorAttribute = new UIStringAttributes
            {
                Link = new NSUrl(PlagiarismLinkType.Author.ToString()),
                Font = Constants.Semibold20,
                ForegroundColor = Constants.R255G34B5,
            };

            _plagiarismAttributedLabel = new Xamarin.TTTAttributedLabel.TTTAttributedLabel();
            _plagiarismAttributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            _plagiarismAttributedLabel.Lines = 0;

            var prop = new NSDictionary();
            _plagiarismAttributedLabel.LinkAttributes = prop;
            _plagiarismAttributedLabel.ActiveLinkAttributes = prop;

            _plagiarismAttributedLabel.UserInteractionEnabled = true;
            _plagiarismAttributedLabel.Enabled = true;
            _plagiarismAttributedLabel.Delegate = new TTTAttributedLabelActionDelegate(TextLinkAction);

            _cancelButton = new UIButton();
            _cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.CancelPublishing).ToUpper(), UIControlState.Normal);
            _cancelButton.Layer.CornerRadius = 25;
            _cancelButton.TitleLabel.Font = Constants.Bold14;
            _cancelButton.TitleLabel.TextColor = Constants.R255G255B255;

            _continueButton = new UIButton();
            _continueButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.ContinuePublishing).ToUpper(), UIControlState.Normal);
            _continueButton.Layer.CornerRadius = 25;
            _continueButton.TitleLabel.Font = Constants.Bold14;
            _continueButton.TitleLabel.TextColor = Constants.R255G255B255;

            mainScroll.AddSubview(photoTitleSeparator);
            mainScroll.AddSubview(_plagiarismAttributedLabel);
            mainScroll.AddSubview(_cancelButton);
            mainScroll.AddSubview(_continueButton);

            _cancelButton.TouchDown += (sender, e) =>
            {
                plagiarismResult.Continue = false;
                NavigationController.PopViewController(true);
            };

            _continueButton.TouchDown += (sender, e) =>
            {
                plagiarismResult.Continue = true;
                NavigationController.PopViewController(true);
            };

            var at = new NSMutableAttributedString();

            if (_model.PlagiarismUsername == AppSettings.User.Login)
            {
                at.Append(new NSAttributedString("We have found a ", noLinkTitleAttribute));
                at.Append(new NSAttributedString(AppSettings.LocalizationManager.GetText(LocalizationKeys.SimilarPhoto).ToLower(), similarAttribute));
                at.Append(new NSAttributedString(" in Steepshot, uploaded by you. We do not recommend you to re-upload photos as it may result in low payouts and reputation loss.", noLinkTitleAttribute));
            }
            else
            {
                at.Append(new NSAttributedString("We have found a ", noLinkTitleAttribute));
                at.Append(new NSAttributedString(AppSettings.LocalizationManager.GetText(LocalizationKeys.SimilarPhoto).ToLower(), similarAttribute));
                at.Append(new NSAttributedString(" in Steepshot, uploaded by ", noLinkTitleAttribute));
                at.Append(new NSAttributedString($"@{_model.PlagiarismUsername}", authorAttribute));
                at.Append(new NSAttributedString(". We do not recommend you to upload other users' photos as it may result in low payouts and reputation loss.", noLinkTitleAttribute));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));

                noLinkTitleAttribute.Font = Constants.Semibold14;
                similarAttribute.Font = Constants.Semibold14;

                at.Append(new NSAttributedString("If you're sure that you are the author of the photo, please flag and/or leave a comment under the ", noLinkTitleAttribute));
                at.Append(new NSAttributedString(AppSettings.LocalizationManager.GetText(LocalizationKeys.Photo).ToLower(), similarAttribute));
                at.Append(new NSAttributedString(" to let other people know they should flag this post.", noLinkTitleAttribute));
            }

            _plagiarismAttributedLabel.SetText(at);

            if (photoView != null)
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoView, 24f);
            else
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoCollection, 24f);

            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            photoTitleSeparator.AutoSetDimension(ALDimension.Height, 1f);
            photoTitleSeparator.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width - _separatorMargin * 2);

            _plagiarismAttributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            _plagiarismAttributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            _plagiarismAttributedLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoTitleSeparator, 24);

            _cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            _cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            _cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _plagiarismAttributedLabel, 34);
            _cancelButton.AutoSetDimension(ALDimension.Height, 50);

            _continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            _continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            _continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 34);
            _continueButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _cancelButton, 10);
            _continueButton.AutoSetDimension(ALDimension.Height, 50);
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
        
        private void TextLinkAction(PlagiarismLinkType type)
        {
            switch (type)
            { 
                case PlagiarismLinkType.Similar:
                    var link = $"@{_model.PlagiarismUsername}/{_model.PlagiarismPermlink}";
                    NavigationController.PushViewController(new PostViewController(link), true);
                    break;
                case PlagiarismLinkType.Author:
                    NavigationController.PushViewController(new ProfileViewController { Username = _model.PlagiarismUsername }, true);
                    break;
            }
        }
    }

    public class TTTAttributedLabelActionDelegate : Xamarin.TTTAttributedLabel.TTTAttributedLabelDelegate
    {
        private Action<PlagiarismLinkType> _linkAction;

        public TTTAttributedLabelActionDelegate(Action<PlagiarismLinkType> linkAction)
        {
            _linkAction = linkAction;
        }

        public override void DidSelectLinkWithURL(Xamarin.TTTAttributedLabel.TTTAttributedLabel label, NSUrl url)
        {
            var type = (PlagiarismLinkType)Enum.Parse(typeof(PlagiarismLinkType), url.ToString(), true);
            _linkAction?.Invoke(type);
        }
    }

    public class PlagiarismResult
    {
        public bool Continue { get; set; }
    }

    public enum PlagiarismLinkType
    { 
        Similar,
        Author
    }
}
