using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using SafariServices;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Xamarin.TTTAttributedLabel;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Views
{
    public class PlagiarismViewController : BaseViewController, ISFSafariViewControllerDelegate
    {
        private UIScrollView mainScroll;
        private readonly Plagiarism _model;
        private readonly TTTAttributedLabel _plagiarismAttributedLabel = new TTTAttributedLabel();
        private UIButton _cancelButton;
        private UIButton _continueButton;
        private PlagiarismResult _plagiarismResult;
        private List<Tuple<NSDictionary, UIImage>> ImageAssets;
        private bool _isinitialized;
        private UIImageView photoView;
        private readonly UIBarButtonItem _guidelines = new UIBarButtonItem();
        private readonly UIButton _leftBarButton = new UIButton();
        private CGSize _cellSize;
        private UICollectionView photoCollection;

        public PlagiarismViewController(List<Tuple<NSDictionary, UIImage>> assets, Plagiarism model, PlagiarismResult plagiarismResult = null)
        {
            ImageAssets = assets;
            _model = model;
            _plagiarismResult = plagiarismResult;
        }

        public override void ViewDidLoad()
        {
            SetupMainScroll();
            SetNavigationBar();
            CreateView();
        }

        public override void ViewWillAppear(bool animated)
        {
            _cancelButton.TouchDown += _cancelButton_TouchDown;
            _continueButton.TouchDown += _continueButton_TouchDown;
            _guidelines.Clicked += OpenGuidelines;
            _leftBarButton.AddTarget(GoBack, UIControlEvent.TouchDown);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            _cancelButton.TouchDown -= _cancelButton_TouchDown;
            _continueButton.TouchDown -= _continueButton_TouchDown;
            _guidelines.Clicked -= OpenGuidelines;
            _leftBarButton.RemoveTarget(GoBack, UIControlEvent.TouchDown);
            base.ViewWillDisappear(animated);
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

        protected UIScrollView CreateScrollView()
        {
            var scroll = new UIScrollView();
            scroll.BackgroundColor = UIColor.White;

            scroll.ShowsVerticalScrollIndicator = true;
            scroll.ScrollEnabled = true;
            scroll.Bounces = true;

            scroll.DelaysContentTouches = true;
            scroll.CanCancelContentTouches = true;
            scroll.ContentMode = UIViewContentMode.ScaleToFill;
            scroll.UserInteractionEnabled = true;

            scroll.Opaque = true;
            scroll.ClipsToBounds = true;

            return scroll;
        }

        private void SetNavigationBar()
        {
            _leftBarButton.SetImage(UIImage.FromBundle("ic_back_arrow"), UIControlState.Normal);
            _leftBarButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.PlagiarismTitle), UIControlState.Normal);
            _leftBarButton.ImageEdgeInsets = new UIEdgeInsets(0, 0, 0, 20);
            _leftBarButton.TitleEdgeInsets = new UIEdgeInsets(0, 20, 0, -20);
            _leftBarButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _leftBarButton.TitleLabel.Font = Constants.Semibold16;
            _leftBarButton.TitleLabel.TextColor = Constants.R15G24B30;
            _leftBarButton.SizeToFit();

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(_leftBarButton);

            _guidelines.Title = AppDelegate.Localization.GetText(LocalizationKeys.GuidelinesForPlagiarism);
           
            var textAttributes = new UITextAttributes();
            textAttributes.Font = Constants.Semibold14;
            textAttributes.TextColor = Constants.R255G34B5;
            _guidelines.SetTitleTextAttributes(textAttributes, UIControlState.Normal);
            _guidelines.SetTitleTextAttributes(textAttributes, UIControlState.Selected);
            NavigationItem.RightBarButtonItem = _guidelines;

            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;
            NavigationController.NavigationBar.Translucent = false;
        }

        protected virtual void GetPostSize()
        {
            if (ImageAssets != null)
                _cellSize = CellHeightCalculator.GetDescriptionPostSize(ImageAssets[0].Item2.Size.Width, ImageAssets[0].Item2.Size.Height, ImageAssets.Count);
        }

        protected void SetupPhoto(CGSize size)
        {
            photoView = new UIImageView();
            photoView.Layer.CornerRadius = 8;
            photoView.ClipsToBounds = true;
            photoView.UserInteractionEnabled = true;
            photoView.ContentMode = UIViewContentMode.ScaleAspectFit;

            mainScroll.AddSubview(photoView);

            photoView.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            photoView.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15f);
            photoView.AutoSetDimension(ALDimension.Width, size.Width);
            photoView.AutoSetDimension(ALDimension.Height, size.Height);
        }

        protected virtual void SetImage()
        {
            if (ImageAssets.Count == 1)
            {
                SetupPhoto(_cellSize);
                photoView.Image = ImageAssets[0].Item2;
            }
            else
            {
                SetupPhotoCollection();

                var galleryCollectionViewSource = new PhotoGalleryViewSource(ImageAssets);
                photoCollection.Source = galleryCollectionViewSource;
                photoCollection.BackgroundColor = UIColor.White;
            }
        }

        protected void SetupPhotoCollection()
        {
            photoCollection = new UICollectionView(CGRect.Null, new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                ItemSize = _cellSize,
                SectionInset = new UIEdgeInsets(0, Constants.DescriptionSectionInset, 0, Constants.DescriptionSectionInset),
                MinimumInteritemSpacing = 10,
            });
            photoCollection.BackgroundColor = UIColor.White;

            mainScroll.AddSubview(photoCollection);

            photoCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            photoCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 30f);
            photoCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            photoCollection.AutoSetDimension(ALDimension.Height, _cellSize.Height);
            photoCollection.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width);

            photoCollection.Bounces = false;
            photoCollection.ShowsHorizontalScrollIndicator = false;
            photoCollection.RegisterClassForCell(typeof(PhotoGalleryCell), nameof(PhotoGalleryCell));
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

            _plagiarismAttributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            _plagiarismAttributedLabel.Lines = 0;

            var prop = new NSDictionary();
            _plagiarismAttributedLabel.LinkAttributes = prop;
            _plagiarismAttributedLabel.ActiveLinkAttributes = prop;

            _plagiarismAttributedLabel.UserInteractionEnabled = true;
            _plagiarismAttributedLabel.Enabled = true;
            _plagiarismAttributedLabel.Delegate = new TTTAttributedLabelActionDelegate(TextLinkAction);

            _cancelButton = new UIButton();
            _cancelButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.CancelPublishing).ToUpper(), UIControlState.Normal);
            _cancelButton.Layer.CornerRadius = 25;
            _cancelButton.TitleLabel.Font = Constants.Bold14;
            _cancelButton.TitleLabel.TextColor = Constants.R255G255B255;

            _continueButton = new UIButton();
            _continueButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.ContinuePublishing).ToUpper(), UIControlState.Normal);
            _continueButton.Layer.CornerRadius = 25;
            _continueButton.TitleLabel.Font = Constants.Bold14;
            _continueButton.TitleLabel.TextColor = Constants.R255G255B255;

            mainScroll.AddSubview(photoTitleSeparator);
            mainScroll.AddSubview(_plagiarismAttributedLabel);
            mainScroll.AddSubview(_cancelButton);
            mainScroll.AddSubview(_continueButton);

            var at = new NSMutableAttributedString();

            if (_model.PlagiarismUsername == AppDelegate.User.Login)
            {
                at.Append(new NSAttributedString("We have found a ", noLinkTitleAttribute));
                at.Append(new NSAttributedString(AppDelegate.Localization.GetText(LocalizationKeys.SimilarPhoto).ToLower(), similarAttribute));
                at.Append(new NSAttributedString(" in Steepshot, uploaded by you. We do not recommend you to re-upload photos as it may result in low payouts and reputation loss.", noLinkTitleAttribute));
            }
            else
            {
                at.Append(new NSAttributedString("We have found a ", noLinkTitleAttribute));
                at.Append(new NSAttributedString(AppDelegate.Localization.GetText(LocalizationKeys.SimilarPhoto).ToLower(), similarAttribute));
                at.Append(new NSAttributedString(" in Steepshot, uploaded by ", noLinkTitleAttribute));
                at.Append(new NSAttributedString($"@{_model.PlagiarismUsername}", authorAttribute));
                at.Append(new NSAttributedString(". We do not recommend you to upload other users' photos as it may result in low payouts and reputation loss.", noLinkTitleAttribute));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));
                at.Append(new NSAttributedString(Environment.NewLine));

                noLinkTitleAttribute.Font = Constants.Semibold14;
                similarAttribute.Font = Constants.Semibold14;

                at.Append(new NSAttributedString("If you're sure that you are the author of the photo, please flag and/or leave a comment under the ", noLinkTitleAttribute));
                at.Append(new NSAttributedString(AppDelegate.Localization.GetText(LocalizationKeys.Photo).ToLower(), similarAttribute));
                at.Append(new NSAttributedString(" to let other people know they should flag this post.", noLinkTitleAttribute));
            }

            _plagiarismAttributedLabel.SetText(at);

            if (photoView != null)
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoView, 24f);
            else
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoCollection, 24f);

            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, Constants.DescriptionSeparatorMargin);
            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, Constants.DescriptionSeparatorMargin);
            photoTitleSeparator.AutoSetDimension(ALDimension.Height, 1f);
            photoTitleSeparator.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width - Constants.DescriptionSeparatorMargin * 2);

            _plagiarismAttributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, Constants.DescriptionSeparatorMargin);
            _plagiarismAttributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, Constants.DescriptionSeparatorMargin);
            _plagiarismAttributedLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoTitleSeparator, 24);

            _cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, Constants.DescriptionSeparatorMargin);
            _cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, Constants.DescriptionSeparatorMargin);
            _cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _plagiarismAttributedLabel, 34);
            _cancelButton.AutoSetDimension(ALDimension.Height, 50);

            _continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, Constants.DescriptionSeparatorMargin);
            _continueButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, Constants.DescriptionSeparatorMargin);
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

        private void _continueButton_TouchDown(object sender, EventArgs e)
        {
            _plagiarismResult.Continue = true;
            NavigationController.PopViewController(true);
        }

        private void _cancelButton_TouchDown(object sender, EventArgs e)
        {
            _plagiarismResult.Continue = false;
            NavigationController.PopViewController(true);
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
