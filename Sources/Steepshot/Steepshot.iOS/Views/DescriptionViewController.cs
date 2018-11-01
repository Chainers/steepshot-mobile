using System;
using Foundation;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
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
using Steepshot.Core.Exceptions;
using Steepshot.iOS.CustomViews;
using AVFoundation;
using Steepshot.iOS.Delegates;
using CoreMedia;

namespace Steepshot.iOS.Views
{
    public partial class DescriptionViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        private const int _photoSize = 900;

        private TimeSpan _postingLimit;
        private PlagiarismResult _plagiarismResult;
        private bool _isSpammer;

        protected List<Tuple<NSDictionary, UIImage>> ImageAssets;
        protected int photoMargin;

        protected UIScrollView mainScroll;
        protected CropView _cropView;
        protected UIImageView titleEditImage;
        protected UIImageView descriptionEditImage;
        protected UILabel tagField;
        protected UIImageView hashtagImage;
        protected UIButton postPhotoButton;
        protected UIActivityIndicatorView loadingView;
        protected NSLayoutConstraint tagsCollectionHeight;
        protected CGSize _cellSize;
        protected bool _isinitialized;

        protected Post post;
        protected PreparePostModel model;
        protected ManualResetEvent mre = new ManualResetEvent(false);
        protected LocalTagsCollectionViewFlowDelegate collectionViewDelegate;
        protected LocalTagsCollectionViewSource collectionviewSource;

        protected UILabel titlePlaceholderLabel;
        protected UILabel descriptionPlaceholderLabel;
        protected UITextView titleTextField;
        protected UITextView descriptionTextField;
        protected UICollectionView tagsCollectionView;
        protected UICollectionView photoCollection;
        protected UIImageView photoView;
        protected VideoView videoContainer;
        protected bool editMode;

        protected readonly UIImageView _rotateButton = new UIImageView();
        protected readonly UIImageView _resizeButton = new UIImageView();
        private readonly MediaType _mediaType;
        private readonly NSUrl _videoUrl;
        private readonly AVAsset _videoAsset;
        private readonly UIDeviceOrientation _rotation;
        private readonly string _imageExtension;

        private PostTitleTextViewDelegate _titleTextViewDelegate = new PostTitleTextViewDelegate();
        private PostTitleTextViewDelegate _descriptionTextViewDelegate = new PostTitleTextViewDelegate(2048);
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();
        private readonly UIBarButtonItem _rightBarButton = new UIBarButtonItem();

        private UITapGestureRecognizer _viewTap;
        private UITapGestureRecognizer _openTagsGestureRecognizer;
        private UITapGestureRecognizer _rotateTap;
        private UITapGestureRecognizer _zoomTap;

        public bool _isFromCamera => ImageAssets?.Count == 1 && ImageAssets[0]?.Item1 == null && _mediaType.Equals(MediaType.Photo);
        private Task<List<MediaModel>> MediaResults;

        public DescriptionViewController() { }

        public DescriptionViewController(List<Tuple<NSDictionary, UIImage>> imageAssets, string extension, UIDeviceOrientation rotation = UIDeviceOrientation.Portrait)
        {
            ImageAssets = imageAssets;
            _imageExtension = extension;
            _rotation = rotation;
            _mediaType = MediaType.Photo;
        }

        public DescriptionViewController(NSUrl videoUrl)
        {
            _videoUrl = videoUrl;
            _videoAsset = AVAsset.FromUrl(_videoUrl);
            _mediaType = MediaType.Video;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _viewTap = new UITapGestureRecognizer(RemoveFocusFromTextFields);
            _openTagsGestureRecognizer = new UITapGestureRecognizer(OpenTagPicker);
            _rotateTap = new UITapGestureRecognizer(RotateTap);
            _zoomTap = new UITapGestureRecognizer(ZoomTap);

            SetupMainScroll();

            tagsCollectionView = new UICollectionView(CGRect.Null, new LeftAlignedCollectionViewFlowLayout());
            tagsCollectionView.ScrollEnabled = false;

            CreateView();

            tagsCollectionView.RegisterClassForCell(typeof(LocalTagCollectionViewCell), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(LocalTagCollectionViewCell), NSBundle.MainBundle), nameof(LocalTagCollectionViewCell));
            collectionviewSource = new LocalTagsCollectionViewSource(editMode);
            collectionViewDelegate = new LocalTagsCollectionViewFlowDelegate(collectionviewSource, UIScreen.MainScreen.Bounds.Width - Constants.DescriptionSeparatorMargin * 2);
            tagsCollectionView.Source = collectionviewSource;
            tagsCollectionView.Delegate = collectionViewDelegate;
            tagsCollectionView.BackgroundColor = UIColor.White;

            SetBackButton();
            SetPlaceholder();

            if (!editMode)
                CheckOnSpam(false);

            if (!_isFromCamera)
                MediaResults = GetMediaResults();
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

        protected virtual void GetPostSize()
        {
            if (ImageAssets != null)
                _cellSize = CellHeightCalculator.GetDescriptionPostSize(ImageAssets[0].Item2.Size.Width, ImageAssets[0].Item2.Size.Height, ImageAssets.Count);
            else
            {
                var videoTrack = _videoAsset.TracksWithMediaType(AVMediaType.Video).First().NaturalSize;
                _cellSize = CellHeightCalculator.GetDescriptionPostSize(videoTrack.Width, videoTrack.Height, 1);
            }
        }

        private void OpenTagPicker()
        {
            RemoveOkButton();
            KeyBoardDownNotification(null);
            NavigationController.PushViewController(new TagsPickerViewController(collectionviewSource, collectionViewDelegate), true);
        }

        public async override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

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
                _resizeButton.AddGestureRecognizer(_zoomTap);
                _rotateButton.AddGestureRecognizer(_rotateTap);
                ((InteractivePopNavigationController)NavigationController).DidEnterBackgroundEvent += VideoViewTapped;
                ((InteractivePopNavigationController)NavigationController).WillEnterForegroundEvent += VideoViewTapped;
            }

            if (_plagiarismResult != null && _plagiarismResult.Continue && !IsMovingToParentViewController)
            {
                await OnPostAsync(true);
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
                _resizeButton.RemoveGestureRecognizer(_zoomTap);
                _rotateButton.RemoveGestureRecognizer(_rotateTap);
                ((InteractivePopNavigationController)NavigationController).DidEnterBackgroundEvent -= VideoViewTapped;
                ((InteractivePopNavigationController)NavigationController).WillEnterForegroundEvent -= VideoViewTapped;
            }
            videoContainer?.Stop();
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (_isFromCamera && IsMovingToParentViewController)
            {
                RotatePhotoIfNeeded();
                _cropView.AdjustImageViewSize(ImageAssets[0].Item2);
                _cropView.ImageView.Image = ImageAssets[0].Item2;
                _cropView.ApplyCriticalScale();
                _cropView.ZoomTap(true, false);
                _cropView.SetScrollViewInsets();
            }
        }

        private void VideoViewTapped()
        {
            if (videoContainer != null)
            {
                if (videoContainer.Player.TimeControlStatus == AVPlayerTimeControlStatus.Playing)
                {
                    videoContainer.Stop();
                }
                else
                {
                    videoContainer.Play();
                }
            }
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

        private void DoneTapped()
        {
            RemoveFocusFromTextFields();
        }

        private async Task<List<MediaModel>> GetMediaResults()
        {
            List<Task<OperationResult<MediaModel>>> uploadTasks = new List<Task<OperationResult<MediaModel>>>();

            if (_mediaType == MediaType.Photo)
            {
                for (int i = 0; i < ImageAssets.Count; i++)
                {
                    var byteArray = await PreparePhoto(ImageAssets[i].Item2, ImageAssets[i].Item1);
                    uploadTasks.Add(UploadMedia(byteArray));
                }
            }
            else
                uploadTasks.Add(UploadMedia(null));

            await Task.WhenAll(uploadTasks);
            return uploadTasks.Select(x => x.Result.Result).ToList();
        }

        private Task<NSData> PreparePhoto(UIImage photo, NSDictionary metadata)
        {
            return Task.Run(() =>
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
                    var editedExifData = MetadataHelper.RemakeMetadata(metadata, photo);
                    var newImageDataWithExif = new NSMutableData();
                    var imageDestination = CGImageDestination.Create(newImageDataWithExif, "public.jpeg", 0);
                    imageDestination.AddImage(new UIImage(byteArray).CGImage, editedExifData);
                    imageDestination.Close();
                    return newImageDataWithExif;
                }
                else
                    return byteArray;
            });
        }

        private async Task<OperationResult<MediaModel>> UploadMedia(NSData byteArray)
        {
            OperationResult<UUIDModel> uploadResult;
            do
            {
                Stream stream;
                if (byteArray == null)
                    stream = new CustomInputStream(new NSInputStream(_videoUrl));
                else
                    stream = byteArray.AsStream();

                var request = new UploadMediaModel(AppDelegate.User.UserInfo, stream, _mediaType == MediaType.Photo ? _imageExtension : ".mp4");
                uploadResult = await Presenter.TryUploadMediaAsync(request).ConfigureAwait(false);
                stream.Dispose();
            } while (!uploadResult.IsSuccess);

            OperationResult<UploadMediaStatusModel> statusResult;

            if (uploadResult.IsSuccess)
                statusResult = await GetMediaStatus(uploadResult.Result).ConfigureAwait(false);
            else
                return new OperationResult<MediaModel>(uploadResult.Exception);

            if (statusResult.IsSuccess && statusResult.Result.Code == UploadMediaCode.Done)
                return await Presenter.TryGetMediaResultAsync(uploadResult.Result);
            else
                return new OperationResult<MediaModel>(statusResult.Exception);
        }

        private async Task<OperationResult<UploadMediaStatusModel>> GetMediaStatus(UUIDModel uuidModel)
        {
            var done = false;
            OperationResult<UploadMediaStatusModel> state;
            do
            {
                state = await Presenter.TryGetMediaStatusAsync(uuidModel).ConfigureAwait(false);
                if (state.IsSuccess)
                {
                    switch (state.Result.Code)
                    {
                        case UploadMediaCode.Done:
                        case UploadMediaCode.FailedToProcess:
                        case UploadMediaCode.FailedToUpload:
                        case UploadMediaCode.FailedToSave:
                            done = true;
                            break;
                        default:
                            await Task.Delay(3000).ConfigureAwait(false);
                            break;
                    }
                }
            } while (!done);

            return state;
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

        private async void PostPhoto(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(titleTextField.Text))
            {
                ShowAlert(LocalizationKeys.EmptyTitleField);
                return;
            }
            RemoveFocusFromTextFields();
            await OnPostAsync(false);
        }

        protected virtual async Task OnPostAsync(bool skipPreparationSteps)
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

            if (MediaResults == null)
                MediaResults = GetMediaResults();

            await MediaResults;

            var mediaResults = MediaResults.Result;

            if (!mediaResults.Any(l => l == null))
            {
                model = new PreparePostModel(AppDelegate.User.UserInfo, AppDelegate.AppInfo.GetModel())
                {
                    Title = titleTextField.Text,
                    Description = titleTextField.Text,
                    Device = "iOS",
                    Tags = collectionviewSource.LocalTags.ToArray(),
                    Media = mediaResults.ToArray(),
                };
                await CreateOrEditPostAsync(skipPreparationSteps);
            }

            EnablePostAndEdit(true);
        }

        protected async Task CreateOrEditPostAsync(bool skipPlagiarismCheck)
        {
            var pushToBlockchainRetry = false;
            do
            {
                if (!skipPlagiarismCheck)
                {
                    var plagiarismCheck = await Presenter.TryPreparePostAsync(model).ConfigureAwait(true);
                    if (plagiarismCheck.IsSuccess)
                    {
                        if (plagiarismCheck.Result.Plagiarism.IsPlagiarism)
                        {
                            _plagiarismResult = new PlagiarismResult();

                            if (_mediaType == MediaType.Video)
                            {
                                var assetIG = new AVAssetImageGenerator(_videoAsset)
                                {
                                    AppliesPreferredTrackTransform = true,
                                    ApertureMode = AVAssetImageGenerator.ApertureModeEncodedPixels
                                };

                                var cmTime = new CMTime(0, 30);
                                var thumbnailImageRef = assetIG.CopyCGImageAtTime(cmTime, out var time, out var error);
                                var firstFrame = new UIImage(thumbnailImageRef);
                                ImageAssets = new List<Tuple<NSDictionary, UIImage>>();
                                ImageAssets.Add(new Tuple<NSDictionary, UIImage>(null, firstFrame));
                            }

                            var plagiarismViewController = new PlagiarismViewController(ImageAssets, plagiarismCheck.Result.Plagiarism, _plagiarismResult);
                            NavigationController.PushViewController(plagiarismViewController, true);
                            return;
                        }
                    }
                }

                pushToBlockchainRetry = false;
                var response = await Presenter.TryCreateOrEditPostAsync(model).ConfigureAwait(true);
                if (!(response != null && response.IsSuccess))
                {
                    ShowDialog(response.Exception, LocalizationKeys.Cancel, LocalizationKeys.Retry,
                           (arg) => { mre.Set(); }, (arg) =>
                            {
                                pushToBlockchainRetry = true;
                                mre.Set();
                            });

                    mre.Reset();
                    await Task.Run(() => { mre.WaitOne(); });
                }
                else
                {
                    ShouldProfileUpdate = true;
                    NavigationController.ViewControllers = new UIViewController[]
                        {NavigationController.ViewControllers[0], this};
                    NavigationController.PopViewController(false);
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
            ImageAssets.Add(new Tuple<NSDictionary, UIImage>(null, _cropView.ImageView.Image));
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
