using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewSources;
using UIKit;
using PureLayout.Net;
using CoreGraphics;
using Steepshot.Core.Localization;
using Steepshot.iOS.Cells;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.CustomViews;

namespace Steepshot.iOS.Views
{
    public partial class DescriptionViewController
    {
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

        private void SetupMainScroll()
        {
            mainScroll = CreateScrollView();
            View.AddSubview(mainScroll);

            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
        }

        private void CreateView()
        {
            GetPostSize();
            SetImage();

            var photoTitleSeparator = new UIView();
            photoTitleSeparator.BackgroundColor = Constants.R245G245B245;

            titleTextField = new UITextView();
            titleTextField.ScrollEnabled = false;
            titleTextField.Font = Constants.Semibold14;
            titleEditImage = new UIImageView();
            titleEditImage.Image = UIImage.FromBundle("ic_edit");

            var titleDescriptionSeparator = new UIView();
            titleDescriptionSeparator.BackgroundColor = Constants.R245G245B245;

            descriptionTextField = new UITextView();
            descriptionTextField.ScrollEnabled = false;
            descriptionTextField.Font = Constants.Regular14;
            descriptionEditImage = new UIImageView();
            descriptionEditImage.Image = UIImage.FromBundle("ic_edit");

            var descriptionHashtagSeparator = new UIView();
            descriptionHashtagSeparator.BackgroundColor = Constants.R245G245B245;

            tagField = new UILabel();
            tagField.Text = "Hashtag";
            tagField.Font = Constants.Regular14;
            tagField.TextColor = Constants.R151G155B158;
            tagField.UserInteractionEnabled = true;

            hashtagImage = new UIImageView();
            hashtagImage.Image = UIImage.FromBundle("ic_hash");

            var hashtagCollectionSeparator = new UIView();
            hashtagCollectionSeparator.BackgroundColor = Constants.R245G245B245;

            postPhotoButton = new UIButton();
            postPhotoButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.PublishButtonText).ToUpper(), UIControlState.Normal);
            postPhotoButton.SetTitle("", UIControlState.Disabled);
            postPhotoButton.Layer.CornerRadius = 25;
            postPhotoButton.TitleLabel.Font = Constants.Semibold14;

            loadingView = new UIActivityIndicatorView();
            loadingView.Color = UIColor.White;
            loadingView.HidesWhenStopped = true;

            mainScroll.Bounces = false;
            mainScroll.AddSubview(photoTitleSeparator);
            mainScroll.AddSubview(titleTextField);
            mainScroll.AddSubview(titleEditImage);
            mainScroll.AddSubview(titleDescriptionSeparator);
            mainScroll.AddSubview(descriptionTextField);
            mainScroll.AddSubview(descriptionEditImage);
            mainScroll.AddSubview(descriptionHashtagSeparator);
            mainScroll.AddSubview(tagField);
            mainScroll.AddSubview(hashtagImage);
            mainScroll.AddSubview(hashtagCollectionSeparator);
            mainScroll.AddSubview(tagsCollectionView);
            mainScroll.AddSubview(postPhotoButton);
            mainScroll.AddSubview(loadingView);

            if (_mediaType == MediaType.Photo)
            {
                if (photoView != null)
                    photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoView, 15f);
                else
                    photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoCollection, 15f);
            }
            else
            {
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, videoContainer, 15f);
            }

            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, SeparatorMargin);
            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, SeparatorMargin);
            photoTitleSeparator.AutoSetDimension(ALDimension.Height, 1f);
            photoTitleSeparator.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width - SeparatorMargin * 2);

            titleTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoTitleSeparator, 17f);
            titleTextField.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator, -5f);

            titleEditImage.AutoSetDimensionsToSize(new CGSize(18, 18));
            titleEditImage.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            titleEditImage.AutoPinEdge(ALEdge.Left, ALEdge.Right, titleTextField, 5f);
            titleEditImage.AutoAlignAxis(ALAxis.Horizontal, titleTextField);

            titleDescriptionSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, titleTextField, 17f);
            titleDescriptionSeparator.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            titleDescriptionSeparator.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            titleDescriptionSeparator.AutoSetDimension(ALDimension.Height, 1f);

            descriptionTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, titleDescriptionSeparator, 17f);
            descriptionTextField.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator, -5f);

            descriptionEditImage.AutoSetDimensionsToSize(new CGSize(18, 18));
            descriptionEditImage.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            descriptionEditImage.AutoPinEdge(ALEdge.Left, ALEdge.Right, descriptionTextField, 5f);
            descriptionEditImage.AutoAlignAxis(ALAxis.Horizontal, descriptionTextField);

            descriptionHashtagSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, descriptionTextField, 17f);
            descriptionHashtagSeparator.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            descriptionHashtagSeparator.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            descriptionHashtagSeparator.AutoSetDimension(ALDimension.Height, 1f);

            tagField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, descriptionHashtagSeparator);
            tagField.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            tagField.AutoSetDimension(ALDimension.Height, 70f);

            hashtagImage.AutoSetDimensionsToSize(new CGSize(15, 17));
            hashtagImage.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            hashtagImage.AutoPinEdge(ALEdge.Left, ALEdge.Right, tagField, 5f);
            hashtagImage.AutoAlignAxis(ALAxis.Horizontal, tagField);

            hashtagCollectionSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagField);
            hashtagCollectionSeparator.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            hashtagCollectionSeparator.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            hashtagCollectionSeparator.AutoSetDimension(ALDimension.Height, 1f);

            tagsCollectionView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, hashtagCollectionSeparator, 25f);
            tagsCollectionView.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            tagsCollectionView.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            tagsCollectionHeight = tagsCollectionView.AutoSetDimension(ALDimension.Height, 0f);

            postPhotoButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagsCollectionView, 40f);
            postPhotoButton.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            postPhotoButton.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            postPhotoButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 35f);
            postPhotoButton.AutoSetDimension(ALDimension.Height, 50f);

            loadingView.AutoAlignAxis(ALAxis.Horizontal, postPhotoButton);
            loadingView.AutoAlignAxis(ALAxis.Vertical, postPhotoButton);
        }

        protected virtual void SetImage()
        {
            if (_mediaType == MediaType.Photo)
            {
                if (ImageAssets.Count == 1)
                {
                    if (!_isFromCamera)
                    {
                        SetupPhoto(_cellSize);
                        photoView.Image = ImageAssets[0].Item2;
                    }
                    else
                    {
                        SetupPhoto(new CGSize(UIScreen.MainScreen.Bounds.Width - 15 * 2, UIScreen.MainScreen.Bounds.Width - 15 * 2));
                        _cropView = new CropView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width - 15 * 2, UIScreen.MainScreen.Bounds.Width - 15 * 2));
                        photoView.AddSubview(_cropView);

                        _resizeButton.Image = UIImage.FromBundle("ic_resize");
                        _resizeButton.UserInteractionEnabled = true;
                        mainScroll.AddSubview(_resizeButton);
                        _resizeButton.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoView, 15f);
                        _resizeButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, photoView, -15f);

                        _rotateButton.Image = UIImage.FromBundle("ic_rotate");
                        _rotateButton.UserInteractionEnabled = true;
                        mainScroll.AddSubview(_rotateButton);
                        _rotateButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, _resizeButton, 15f);
                        _rotateButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, photoView, -15f);
                    }
                }
                else
                {
                    SetupPhotoCollection();

                    var galleryCollectionViewSource = new PhotoGalleryViewSource(ImageAssets);
                    photoCollection.Source = galleryCollectionViewSource;
                    photoCollection.BackgroundColor = UIColor.White;
                }
            }
            else
            {
                videoContainer = new VideoView(true);
                videoContainer.ClipsToBounds = true;
                videoContainer.Layer.CornerRadius = 8;
                mainScroll.AddSubview(videoContainer);

                videoContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
                videoContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15f);
                videoContainer.AutoSetDimensionsToSize(new CGSize(_cellSize.Width, _cellSize.Height));
                videoContainer.ChangeItem(_videoUrl);

                _statusImage = new UIImageView();
                _statusImage.Image = UIImage.FromBundle("ic_play");
                videoContainer.AddSubview(_statusImage);
                _statusImage.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 10);
                _statusImage.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);
                _statusImage.AutoSetDimensionsToSize(new CGSize(32, 32));
            }
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

        protected void SetupPhotoCollection()
        {
            photoCollection = new UICollectionView(CGRect.Null, new UICollectionViewFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                ItemSize = _cellSize,
                SectionInset = new UIEdgeInsets(0, sectionInset, 0, sectionInset),
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

        private void SetBackButton()
        {
            _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            NavigationItem.LeftBarButtonItem = _leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppDelegate.Localization.GetText(LocalizationKeys.PostSettings);
            NavigationController.NavigationBar.Translucent = false;
        }

        protected void ResizeView()
        {
            tagsCollectionView.LayoutIfNeeded();
            var collectionContentSize = tagsCollectionView.ContentSize;
            tagsCollectionHeight.Constant = collectionContentSize.Height;
        }

        private void AddOkButton()
        {
            _rightBarButton.Title = "Ok";
            _rightBarButton.Clicked += DoneTapped;
            NavigationItem.RightBarButtonItem = _rightBarButton;
        }

        private void RemoveOkButton()
        {
            _rightBarButton.Clicked -= DoneTapped;
            NavigationItem.RightBarButtonItem = null;
        }

        private void RemoveFocusFromTextFields()
        {
            descriptionTextField.ResignFirstResponder();
            titleTextField.ResignFirstResponder();
            tagField.ResignFirstResponder();
            RemoveOkButton();
        }

        private void SetPlaceholder()
        {
            titleTextField.Delegate = _titleTextViewDelegate;

            titlePlaceholderLabel = new UILabel();
            titlePlaceholderLabel.Text = "Enter a title of your photo";
            titlePlaceholderLabel.SizeToFit();
            titlePlaceholderLabel.Font = Constants.Regular14;
            titlePlaceholderLabel.TextColor = Constants.R151G155B158;
            titlePlaceholderLabel.Hidden = false;

            var labelX = titleTextField.TextContainerInset.Left;
            var labelY = titleTextField.TextContainerInset.Top;
            var labelWidth = titlePlaceholderLabel.Frame.Width;
            var labelHeight = titlePlaceholderLabel.Frame.Height;

            titlePlaceholderLabel.Frame = new CGRect(5, labelY, labelWidth, labelHeight);

            titleTextField.AddSubview(titlePlaceholderLabel);
            _titleTextViewDelegate.Placeholder = titlePlaceholderLabel;

            descriptionTextField.Delegate = _descriptionTextViewDelegate;

            descriptionPlaceholderLabel = new UILabel();
            descriptionPlaceholderLabel.Text = "Enter a description of your photo";
            descriptionPlaceholderLabel.SizeToFit();
            descriptionPlaceholderLabel.Font = Constants.Regular14;
            descriptionPlaceholderLabel.TextColor = Constants.R151G155B158;
            descriptionPlaceholderLabel.Hidden = false;

            var descLabelX = descriptionTextField.TextContainerInset.Left;
            var descLabelY = descriptionTextField.TextContainerInset.Top;
            var descLabelWidth = descriptionPlaceholderLabel.Frame.Width;
            var descLabelHeight = descriptionPlaceholderLabel.Frame.Height;

            descriptionPlaceholderLabel.Frame = new CGRect(5, descLabelY, descLabelWidth, descLabelHeight);

            descriptionTextField.AddSubview(descriptionPlaceholderLabel);
            _descriptionTextViewDelegate.Placeholder = descriptionPlaceholderLabel;
        }
    }
}
