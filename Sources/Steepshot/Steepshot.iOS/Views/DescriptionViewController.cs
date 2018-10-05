using System;
using Foundation;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using CoreGraphics;
using System.Threading.Tasks;
using Constants = Steepshot.iOS.Helpers.Constants;
using System.Threading;
using Steepshot.iOS.Helpers;
using Steepshot.Core.Models.Common;
using System.Collections.Generic;
using Steepshot.Core.Models.Enums;
using System.IO;
using System.Linq;
using Steepshot.Core.Localization;
using ImageIO;
using PureLayout.Net;
using Steepshot.Core.Exceptions;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Delegates;

namespace Steepshot.iOS.Views
{
    public class DescriptionViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        private const int _photoSize = 900; //kb
        private TimeSpan _postingLimit;
        private readonly UIDeviceOrientation _rotation;
        private PlagiarismResult _plagiarismResult;
        protected List<Tuple<NSDictionary, UIImage>> ImageAssets;
        protected nfloat SeparatorMargin = 30;
        private nfloat _photoViewSide;
        protected int photoMargin;

        private readonly string _imageExtension;
        private bool _isSpammer;

        protected UIScrollView mainScroll;
        protected CropView _cropView;
        protected UIImageView titleEditImage;
        protected UIImageView descriptionEditImage;
        protected UILabel tagField;
        protected UIImageView hashtagImage;
        protected UIButton postPhotoButton;
        protected UIActivityIndicatorView loadingView;
        protected NSLayoutConstraint tagsCollectionHeight;
        protected readonly UIImageView _rotateButton = new UIImageView();
        protected readonly UIImageView _resizeButton = new UIImageView();
        protected bool _isinitialized;
        protected CGSize _cellSize;
        protected const int cellSide = 160;
        protected const int sectionInset = 15;

        protected Post post;
        protected PreparePostModel model;
        protected ManualResetEvent mre;
        protected LocalTagsCollectionViewFlowDelegate collectionViewDelegate;
        protected LocalTagsCollectionViewSource collectionviewSource;

        protected UILabel titlePlaceholderLabel;
        protected UILabel descriptionPlaceholderLabel;
        protected UITextView titleTextField;
        protected UITextView descriptionTextField;
        protected UICollectionView tagsCollectionView;
        protected UICollectionView photoCollection;
        protected UIImageView photoView;
        protected bool editMode;

        public bool _isFromCamera => ImageAssets?.Count == 1 && ImageAssets[0]?.Item1 == null;

        private PostTitleTextViewDelegate _titleTextViewDelegate = new PostTitleTextViewDelegate();
        private PostTitleTextViewDelegate _descriptionTextViewDelegate = new PostTitleTextViewDelegate(2048);
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();
        private readonly UIBarButtonItem _rightBarButton = new UIBarButtonItem();

        private readonly UITapGestureRecognizer _viewTap;
        private readonly UITapGestureRecognizer _openTagsGestureRecognizer;
        private readonly UITapGestureRecognizer rotateTap;
        private readonly UITapGestureRecognizer zoomTap;


        public DescriptionViewController() { }

        public DescriptionViewController(List<Tuple<NSDictionary, UIImage>> imageAssets, string extension, UIDeviceOrientation rotation = UIDeviceOrientation.Portrait)
        {
            ImageAssets = imageAssets;
            _imageExtension = extension;
            _rotation = rotation;

            _viewTap = new UITapGestureRecognizer(RemoveFocusFromTextFields);
            _openTagsGestureRecognizer = new UITapGestureRecognizer(OpenTagPicker);
            rotateTap = new UITapGestureRecognizer(RotateTap);
            zoomTap = new UITapGestureRecognizer(ZoomTap);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupMainScroll();

            tagsCollectionView = new UICollectionView(CGRect.Null, new LeftAlignedCollectionViewFlowLayout());
            tagsCollectionView.ScrollEnabled = false;

            CreateView();

            tagsCollectionView.RegisterClassForCell(typeof(LocalTagCollectionViewCell), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(LocalTagCollectionViewCell), NSBundle.MainBundle), nameof(LocalTagCollectionViewCell));
            collectionviewSource = new LocalTagsCollectionViewSource(editMode);
            collectionViewDelegate = new LocalTagsCollectionViewFlowDelegate(collectionviewSource, UIScreen.MainScreen.Bounds.Width - SeparatorMargin * 2);
            tagsCollectionView.Source = collectionviewSource;
            tagsCollectionView.Delegate = collectionViewDelegate;
            tagsCollectionView.BackgroundColor = UIColor.White;

            SetBackButton();
            SetPlaceholder();

            if (!editMode)
                CheckOnSpam(false);
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            var kbSize = UIKeyboard.FrameEndFromNotification(notification);
            var contentInsets = new UIEdgeInsets(0, 0, kbSize.Height, 0);
            mainScroll.ContentInset = contentInsets;
            mainScroll.ScrollIndicatorInsets = contentInsets;
            mainScroll.ScrollRectToVisible(Activeview.Frame, true);
        }

        protected override void KeyBoardDownNotification(NSNotification notification)
        {
            var contentInsets = new UIEdgeInsets(0, 0, 0, 0);
            mainScroll.ContentInset = contentInsets;
            mainScroll.ScrollIndicatorInsets = contentInsets;
            View.LayoutSubviews();
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

            if (photoView != null)
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoView, 15f);
            else
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoCollection, 15f);

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

        protected virtual void GetPostSize()
        {
            GetPostSize(ImageAssets[0].Item2.Size.Width, ImageAssets[0].Item2.Size.Height, ImageAssets.Count);
        }

        protected void GetPostSize(nfloat width, nfloat height, int listCount)
        {
            if (height > width)
            {
                var ratio = width / height;
                if (listCount == 1)
                {
                    photoMargin = 15;
                    _cellSize = new CGSize(UIScreen.MainScreen.Bounds.Width - SeparatorMargin * 2, (UIScreen.MainScreen.Bounds.Width - SeparatorMargin * 2) / ratio);
                }
                else
                    _cellSize = new CGSize(cellSide * ratio, cellSide);
            }
            else
            {
                var ratio = height / width;
                if (listCount == 1)
                {
                    photoMargin = 15;
                    _cellSize = new CGSize(UIScreen.MainScreen.Bounds.Width - photoMargin * 2, (UIScreen.MainScreen.Bounds.Width - photoMargin * 2) * ratio);
                }
                else
                    _cellSize = new CGSize(UIScreen.MainScreen.Bounds.Width - sectionInset * 2, (UIScreen.MainScreen.Bounds.Width - sectionInset * 2) * ratio);
            }
        }

        protected virtual void SetImage()
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

        private void OpenTagPicker()
        {
            RemoveOkButton();
            KeyBoardDownNotification(null);
            NavigationController.PushViewController(new TagsPickerViewController(collectionviewSource, collectionViewDelegate), true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (_plagiarismResult != null && _plagiarismResult.Continue && !IsMovingToParentViewController)
            {
                OnPostAsync(true);
            }

            collectionviewSource.CellAction += CollectionCellAction;

            if (!IsMovingToParentViewController)
            {
                tagsCollectionView.ReloadData();
                ResizeView();
            }
            else
            {
                postPhotoButton.TouchDown += PostPhoto;
                _titleTextViewDelegate.EditingStartedAction += _titleTextViewDelegate_EditingStartedAction;
                _descriptionTextViewDelegate.EditingStartedAction += _descriptionTextViewDelegate_EditingStartedAction;
                _leftBarButton.Clicked += GoBack;
                View.AddGestureRecognizer(_viewTap);
                tagField.AddGestureRecognizer(_openTagsGestureRecognizer);
                _resizeButton.AddGestureRecognizer(zoomTap);
                _rotateButton.AddGestureRecognizer(rotateTap);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            collectionviewSource.CellAction -= CollectionCellAction;
            if (IsMovingFromParentViewController)
            {
                postPhotoButton.TouchDown -= PostPhoto;
                _titleTextViewDelegate.EditingStartedAction = null;
                _descriptionTextViewDelegate.EditingStartedAction = null;
                _leftBarButton.Clicked -= GoBack;
                View.RemoveGestureRecognizer(_viewTap);
                tagField.RemoveGestureRecognizer(_openTagsGestureRecognizer);
                _resizeButton.RemoveGestureRecognizer(zoomTap);
                _rotateButton.RemoveGestureRecognizer(rotateTap);
            }
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (_isFromCamera && IsMovingToParentViewController)
            {
                RotatePhotoIfNeeded();
                _cropView.AdjustImageViewSize(ImageAssets[0].Item2);
                _cropView.imageView.Image = ImageAssets[0].Item2;
                _cropView.ApplyCriticalScale();
                _cropView.ZoomTap(true, false);
                _cropView.SetScrollViewInsets();
            }
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

        private void EditingStartedAction()
        {
            AddOkButton();
        }

        private void _descriptionTextViewDelegate_EditingStartedAction()
        {
            Activeview = descriptionTextField;
        }

        private void _titleTextViewDelegate_EditingStartedAction()
        {
            Activeview = titleTextField;
        }

        private void CollectionCellAction(ActionType type, string tag)
        {
            RemoveTag(tag);
        }

        private void RemoveTag(string tag)
        {
            collectionviewSource.LocalTags.Remove(tag);
            collectionViewDelegate.GenerateVariables();
            tagsCollectionView.ReloadData();
            ResizeView();
        }

        public override void ViewDidLayoutSubviews()
        {
            if (!_isinitialized)
            {
                postPhotoButton.LayoutIfNeeded();
                Constants.CreateGradient(postPhotoButton, 25);
                Constants.CreateShadow(postPhotoButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
                _isinitialized = true;
            }
        }

        protected void ResizeView()
        {
            tagsCollectionView.LayoutIfNeeded();
            var collectionContentSize = tagsCollectionView.ContentSize;
            tagsCollectionHeight.Constant = collectionContentSize.Height;
        }

        private void SetBackButton()
        {
            _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            NavigationItem.LeftBarButtonItem = _leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppDelegate.Localization.GetText(LocalizationKeys.PostSettings);
            NavigationController.NavigationBar.Translucent = false;
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

        private void DoneTapped()
        {
            RemoveFocusFromTextFields();
        }

        private void RemoveFocusFromTextFields()
        {
            descriptionTextField.ResignFirstResponder();
            titleTextField.ResignFirstResponder();
            tagField.ResignFirstResponder();
            RemoveOkButton();
        }

        private async Task<OperationResult<MediaModel>> UploadPhoto(UIImage photo, NSDictionary metadata)
        {
            Stream stream = null;
            try
            {
                var compression = 1f;
                var maxCompression = 0.1f;
                int maxFileSize = _photoSize * 1024;

                var byteArray = photo.AsJPEG(compression);

                while (byteArray.Count() > maxFileSize && compression > maxCompression)
                {
                    compression -= 0.1f;
                    byteArray = photo.AsJPEG(compression);
                }

                if (metadata != null)
                {
                    //exif setup
                    var editedExifData = RemakeMetadata(metadata, photo);
                    var newImageDataWithExif = new NSMutableData();
                    var imageDestination = CGImageDestination.Create(newImageDataWithExif, "public.jpeg", 0);
                    imageDestination.AddImage(new UIImage(byteArray).CGImage, editedExifData);
                    imageDestination.Close();
                    stream = newImageDataWithExif.AsStream();
                }
                else
                    stream = byteArray.AsStream();

                var request = new UploadMediaModel(AppDelegate.User.UserInfo, stream, _imageExtension);
                var serverResult = await Presenter.TryUploadMediaAsync(request);
                if (!serverResult.IsSuccess)
                    return new OperationResult<MediaModel>(serverResult.Exception);

                var uuidModel = serverResult.Result;
                var done = false;
                do
                {
                    var state = await Presenter.TryGetMediaStatusAsync(uuidModel);
                    if (state.IsSuccess)
                    {
                        switch (state.Result.Code)
                        {
                            case UploadMediaCode.Done:
                                done = true;
                                break;

                            case UploadMediaCode.FailedToProcess:
                            case UploadMediaCode.FailedToUpload:
                            case UploadMediaCode.FailedToSave:
                                return new OperationResult<MediaModel>(new Exception(state.Result.Message));

                            default:
                                await Task.Delay(3000);
                                break;
                        }
                    }
                } while (!done);

                return await Presenter.TryGetMediaResultAsync(uuidModel);
            }
            catch (Exception ex)
            {
                return new OperationResult<MediaModel>(new InternalException(LocalizationKeys.PhotoProcessingError, ex));
            }
            finally
            {
                stream?.Flush();
                stream?.Dispose();
            }
        }

        private NSDictionary RemakeMetadata(NSDictionary metadata, UIImage photo)
        {
            var keys = new List<object>();
            var values = new List<object>();

            foreach (var item in metadata)
            {
                keys.Add(item.Key);
                switch (item.Key.ToString())
                {
                    case "Orientation":
                        values.Add(new NSNumber(1));
                        break;
                    case "PixelHeight":
                        values.Add(photo.Size.Height);
                        break;
                    case "PixelWidth":
                        values.Add(photo.Size.Width);
                        break;
                    case "{TIFF}":
                        values.Add(RemakeMetadata((NSDictionary)item.Value, photo));
                        break;
                    default:
                        values.Add(item.Value);
                        break;
                }
            }
            return NSDictionary.FromObjectsAndKeys(values.ToArray(), keys.ToArray());
        }

        private async Task CheckOnSpam(bool disableEditing)
        {
            EnablePostAndEdit(false, disableEditing);
            _isSpammer = false;

            var spamCheck = await Presenter.TryCheckForSpamAsync(AppDelegate.User.Login);
            EnablePostAndEdit(true);

            if (spamCheck.IsSuccess)
            {
                if (!spamCheck.Result.IsSpam)
                {
                    if (spamCheck.Result.WaitingTime > 0)
                    {
                        _isSpammer = true;
                        _postingLimit = TimeSpan.FromMinutes(5);
                        StartPostTimer((int)spamCheck.Result.WaitingTime);
                        ShowAlert(LocalizationKeys.Posts5minLimit);
                    }
                }
                else
                {
                    // more than 15 posts
                    _isSpammer = true;
                    _postingLimit = TimeSpan.FromHours(24);
                    StartPostTimer((int)spamCheck.Result.WaitingTime);
                    ShowAlert(LocalizationKeys.PostsDayLimit);
                }
            }
        }

        private async void StartPostTimer(int startSeconds)
        {
            string timeFormat;
            var timepassed = _postingLimit - TimeSpan.FromSeconds(startSeconds);
            postPhotoButton.UserInteractionEnabled = false;

            while (timepassed < _postingLimit)
            {
                UIView.PerformWithoutAnimation(() =>
                {
                    timeFormat = (_postingLimit - timepassed).TotalHours >= 1 ? "hh\\:mm\\:ss" : "mm\\:ss";
                    postPhotoButton.SetTitle((_postingLimit - timepassed).ToString(timeFormat), UIControlState.Normal);
                    postPhotoButton.LayoutIfNeeded();
                });
                await Task.Delay(1000);

                timepassed = timepassed.Add(TimeSpan.FromSeconds(1));
            }

            _isSpammer = false;
            postPhotoButton.UserInteractionEnabled = true;
            postPhotoButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.PublishButtonText).ToUpper(), UIControlState.Normal);
        }

        private void PostPhoto(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(titleTextField.Text))
            {
                ShowAlert(LocalizationKeys.EmptyTitleField);
                return;
            }

            RemoveFocusFromTextFields();

            OnPostAsync(false);
        }

        protected virtual async void OnPostAsync(bool skipPreparationSteps)
        {
            if (!skipPreparationSteps)
            {
                await CheckOnSpam(true);

                if (_isSpammer)
                    return;
            }

            EnablePostAndEdit(false);

            if (_isFromCamera && !skipPreparationSteps)
            {
                var croppedPhoto = _cropView.CropImage(new SavedPhoto(null, ImageAssets[0].Item2, _cropView.ContentOffset) { OriginalImageSize = _cropView.originalImageSize, Scale = _cropView.ZoomScale });
                ImageAssets.RemoveAt(0);
                ImageAssets.Add(new Tuple<NSDictionary, UIImage>(null, croppedPhoto));
            }

            await Task.Run(() =>
            {
                try
                {
                    var shouldReturn = false;
                    string title = null;
                    string description = null;
                    IList<string> tags = null;

                    InvokeOnMainThread(() =>
                    {
                        title = titleTextField.Text;
                        description = descriptionTextField.Text;
                        tags = collectionviewSource.LocalTags;
                    });

                    mre = new ManualResetEvent(false);

                    if (!skipPreparationSteps)
                    {
                        var photoUploadRetry = false;
                        OperationResult<MediaModel>[] photoUploadResponse = new OperationResult<MediaModel>[ImageAssets.Count];
                        do
                        {
                            photoUploadRetry = false;
                            for (int i = 0; i < ImageAssets.Count; i++)
                            {
                                photoUploadResponse[i] = UploadPhoto(ImageAssets[i].Item2, ImageAssets[i].Item1).Result;
                            }

                            if (photoUploadResponse.Any(r => r.IsSuccess == false))
                            {
                                InvokeOnMainThread(() =>
                                {
                                    //Remake this
                                    ShowDialog(photoUploadResponse[0].Exception, LocalizationKeys.Cancel,
                                        LocalizationKeys.Retry, (arg) =>
                                        {
                                            shouldReturn = true;
                                            mre.Set();
                                        }, (arg) =>
                                        {
                                            photoUploadRetry = true;
                                            mre.Set();
                                        });
                                });

                                mre.Reset();
                                mre.WaitOne();
                            }
                        } while (photoUploadRetry);

                        if (shouldReturn)
                            return;

                        model = new PreparePostModel(AppDelegate.User.UserInfo, AppDelegate.AppInfo.GetModel())
                        {
                            Title = title,
                            Description = description,
                            Device = "iOS",

                            Tags = tags.ToArray(),
                            Media = photoUploadResponse.Select(r => r.Result).ToArray(),
                        };
                    }

                    CreateOrEditPost(skipPreparationSteps);
                }
                catch (Exception ex)
                {
                    AppDelegate.Logger.WarningAsync(ex);
                }
                finally
                {
                    InvokeOnMainThread(() => { EnablePostAndEdit(true); });
                }
            });
        }

        protected void CreateOrEditPost(bool skipPlagiarismCheck)
        {
            var pushToBlockchainRetry = false;
            do
            {
                if (!skipPlagiarismCheck)
                {
                    var plagiarismCheck = Presenter.TryCheckForPlagiarismAsync(model).Result;
                    if (plagiarismCheck.IsSuccess)
                    {
                        if (plagiarismCheck.Result.plagiarism.IsPlagiarism)
                        {
                            InvokeOnMainThread(() =>
                            {
                                _plagiarismResult = new PlagiarismResult();
                                var plagiarismViewController = new PlagiarismViewController(ImageAssets, plagiarismCheck.Result.plagiarism, _plagiarismResult);
                                NavigationController.PushViewController(plagiarismViewController, true);
                            });

                            return;
                        }
                    }
                }

                pushToBlockchainRetry = false;
                var response = Presenter.TryCreateOrEditPostAsync(model).Result;
                if (!(response != null && response.IsSuccess))
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowDialog(response.Exception, LocalizationKeys.Cancel, LocalizationKeys.Retry,
                            (arg) => { mre.Set(); }, (arg) =>
                            {
                                pushToBlockchainRetry = true;
                                mre.Set();
                            });
                    });

                    mre.Reset();
                    mre.WaitOne();
                }
                else
                {
                    InvokeOnMainThread(() =>
                    {
                        ShouldProfileUpdate = true;
                        NavigationController.ViewControllers = new UIViewController[]
                            {NavigationController.ViewControllers[0], this};
                        NavigationController.PopViewController(false);
                    });
                }
            } while (pushToBlockchainRetry);
        }

        private void RotatePhotoIfNeeded()
        {
            if (_rotation == UIDeviceOrientation.Portrait || _rotation == UIDeviceOrientation.Unknown)
                return;

            UIImageOrientation orientation;

            switch (_rotation)
            {
                case UIDeviceOrientation.Portrait:
                    orientation = UIImageOrientation.Up;
                    break;
                case UIDeviceOrientation.PortraitUpsideDown:
                    orientation = UIImageOrientation.Down;
                    break;
                case UIDeviceOrientation.LandscapeLeft:
                    orientation = UIImageOrientation.Left;
                    break;
                case UIDeviceOrientation.LandscapeRight:
                    orientation = UIImageOrientation.Right;
                    break;
                default:
                    orientation = UIImageOrientation.Up;
                    break;
            }

            var rotated = ImageHelper.RotateImage(ImageAssets[0].Item2, orientation);
            ImageAssets.RemoveAt(0);
            ImageAssets.Add(new Tuple<NSDictionary, UIImage>(null, rotated));
        }

        private void ZoomTap()
        {
            _cropView.ZoomTap(false);
        }

        private void RotateTap()
        {
            UIView.Animate(0.15, () =>
            {
                _rotateButton.Alpha = 0.6f;
            }, () =>
            {
                UIView.Animate(0.15, () =>
                {
                    _rotateButton.Alpha = 1f;
                }, null);
            });

            _cropView.RotateTap();

            ImageAssets.RemoveAt(0);
            ImageAssets.Add(new Tuple<NSDictionary, UIImage>(null, _cropView.imageView.Image));
            _cropView.ApplyCriticalScale();
        }

        protected void EnablePostAndEdit(bool enabled, bool enableFields = true)
        {
            if (enabled)
                loadingView.StopAnimating();
            else
                loadingView.StartAnimating();

            if (_isFromCamera)
            {
                _rotateButton.UserInteractionEnabled = enabled;
                _resizeButton.UserInteractionEnabled = enabled;
                photoView.UserInteractionEnabled = enabled;
            }
            postPhotoButton.Enabled = enabled;

            if (enableFields)
            {
                titleTextField.UserInteractionEnabled = enabled;
                descriptionTextField.UserInteractionEnabled = enabled;
                tagField.UserInteractionEnabled = enabled;
                tagsCollectionView.UserInteractionEnabled = enabled;
            }
        }

        protected override void GoBack(object sender, EventArgs e)
        {
            Presenter.TasksCancel();
            NavigationController.PopViewController(true);
        }

        private void DoneTapped(object sender, EventArgs e)
        {
            DoneTapped();
        }
    }

    public class LeftAlignedCollectionViewFlowLayout : UICollectionViewFlowLayout
    {
        public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
        {
            var attributes = base.LayoutAttributesForElementsInRect(rect);

            var leftMargin = SectionInset.Left;
            double maxY = -1.0f;

            foreach (var layoutAttribute in attributes)
            {
                if (layoutAttribute.Frame.Y >= maxY)
                {
                    leftMargin = SectionInset.Left;
                }
                layoutAttribute.Frame = new CGRect(new CGPoint(leftMargin, layoutAttribute.Frame.Y), layoutAttribute.Frame.Size);

                leftMargin += layoutAttribute.Frame.Width + MinimumInteritemSpacing;
                maxY = Math.Max(layoutAttribute.Frame.GetMaxY(), maxY);
            }
            return attributes;
        }
    }
}
