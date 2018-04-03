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
using Steepshot.Core.Models;
using System.Threading;
using Steepshot.iOS.Helpers;
using Steepshot.Core.Models.Common;
using System.Collections.Generic;
using Steepshot.Core.Models.Enums;
using System.IO;
using System.Linq;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using ImageIO;
using PureLayout.Net;

namespace Steepshot.iOS.Views
{
    public partial class DescriptionViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        private const int _photoSize = 900; //kb
        private readonly TimeSpan PostingLimit = TimeSpan.FromMinutes(5);
        private UIDeviceOrientation _rotation;
        private LocalTagsCollectionViewFlowDelegate _collectionViewDelegate;
        private LocalTagsCollectionViewSource _collectionviewSource;
        private TagsTableViewSource _tableSource;
        private NSDictionary _metadata;
        private List<Tuple<NSDictionary, UIImage>> ImageAssets;
        private Timer _timer;
        private string ImageExtension;
        private string _previousQuery;
        private bool _isSpammer;
        public bool _isFromCamera => ImageAssets.Count == 1 && ImageAssets[0].Item1 == null;

        private UICollectionView photoCollection;
        private UIImageView photoView;
        private UITextView titleTextField;
        private UIImageView titleEditImage;
        private UITextView descriptionTextField;
        private UIImageView descriptionEditImage;
        private UITextField tagField;
        private UIImageView hashtagImage;
        private UICollectionView tagsCollectionView;
        private UIButton postPhotoButton;
        //private UITableView tagsTableView;
        private UIActivityIndicatorView loadingView;

        public DescriptionViewController(List<Tuple<NSDictionary, UIImage>> imageAssets, string extension, UIDeviceOrientation rotation = UIDeviceOrientation.Portrait)
        {
            ImageAssets = imageAssets;
            ImageExtension = extension;
            _rotation = rotation;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            tagsCollectionView = new UICollectionView(CGRect.Null, new LeftAlignedCollectionViewFlowLayout());
            tagsCollectionView.ScrollEnabled = false;
            //tagsCollectionView.TranslatesAutoresizingMaskIntoConstraints = false;

            //tagsTableView = new UITableView();

            CreateView();



            tagsCollectionView.RegisterClassForCell(typeof(LocalTagCollectionViewCell), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(LocalTagCollectionViewCell), NSBundle.MainBundle), nameof(LocalTagCollectionViewCell));
            _collectionviewSource = new LocalTagsCollectionViewSource();
            _collectionviewSource.CellAction += CollectionCellAction;
            _collectionViewDelegate = new LocalTagsCollectionViewFlowDelegate(_collectionviewSource, UIScreen.MainScreen.Bounds.Width -_separatorMargin * 2);
            tagsCollectionView.Source = _collectionviewSource;
            tagsCollectionView.Delegate = _collectionViewDelegate;
            tagsCollectionView.BackgroundColor = UIColor.White;
            Activeview = mainScroll;
            /*
            _tableSource = new TagsTableViewSource(_presenter);
            _tableSource.CellAction += TableCellAction;
            tagsTableView.Source = _tableSource;
            tagsTableView.LayoutMargins = UIEdgeInsets.Zero;
            tagsTableView.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTableView.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTableView.RowHeight = 65f;
*/
            //mainScrollBottom.Active = false;
            //mainScrollHeight.Constant = 100;
            //mainScrollHeight.Active = true;



            tagField.Delegate = new TagFieldDelegate(DoneTapped);
            tagField.EditingChanged += EditingDidChange;
            tagField.EditingDidBegin += EditingDidBegin;
            tagField.EditingDidEnd += EditingDidEnd;

            var tap = new UITapGestureRecognizer(RemoveFocusFromTextFields);
            View.AddGestureRecognizer(tap);
            

            _presenter.SourceChanged += SourceChanged;
            _timer = new Timer(OnTimer);

            SetBackButton();
            //SearchTextChanged();
            SetPlaceholder();
            //CheckOnSpam();
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            var kbSize = UIKeyboard.FrameEndFromNotification(notification);
            //if (!tagField.IsFirstResponder)
            //{
                var contentInsets = new UIEdgeInsets(0, 0, kbSize.Height, 0);
                mainScroll.ContentInset = contentInsets;
                mainScroll.ScrollIndicatorInsets = contentInsets;

                // If active text field is hidden by keyboard, scroll it so it's visible
                // Your app might not need or want this behavior.
                //CGRect aRect = self.view.frame;
                //aRect.size.height -= kbSize.Height;
                //if (!CGRectContainsPoint(aRect, activeField.frame.origin))
                //{
                mainScroll.ScrollRectToVisible(Activeview.Frame, true);
                //}
            //}
            //tableHeight.Constant = UIScreen.MainScreen.Bounds.Height - NavigationController.NavigationBar.Frame.Bottom - kbSize.Height - tagField.Frame.Bottom;
        }

        protected override void KeyBoardDownNotification(NSNotification notification)
        {
            var contentInsets = new UIEdgeInsets(0, 0, 0, 0);
            mainScroll.ContentInset = contentInsets;
            mainScroll.ScrollIndicatorInsets = contentInsets;
        }

        NSLayoutConstraint toTop;
        NSLayoutConstraint norm;

        private nfloat _separatorMargin = 30;

        private void CreateView()
        {
            if (ImageAssets.Count == 1)
            {

                photoView = new UIImageView();
                photoView.ContentMode = UIViewContentMode.ScaleAspectFill;
                photoView.Layer.CornerRadius = 8;
                photoView.ClipsToBounds = true;
                photoView.Image = ImageAssets[0].Item2;
                mainScroll.AddSubview(photoView);

                photoView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
                photoView.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15f);
                photoView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15f);
                photoView.AutoMatchDimension(ALDimension.Height, ALDimension.Width, photoView);
                var photoMargin = 15;
                var photoViewSide = UIScreen.MainScreen.Bounds.Width - photoMargin * 2;
                photoView.AutoSetDimension(ALDimension.Width, photoViewSide);
            }
            else
            {
                photoCollection = new UICollectionView(CGRect.Null, new UICollectionViewFlowLayout()
                {
                    ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                    ItemSize = new CGSize(160, 160),
                    SectionInset = new UIEdgeInsets(0, 15, 0, 0),
                    MinimumInteritemSpacing = 10,
                });
                mainScroll.AddSubview(photoCollection);

                photoCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
                photoCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 30f);
                photoCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
                photoCollection.AutoSetDimension(ALDimension.Height, 160f);
                photoCollection.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width);

                photoCollection.Bounces = false;
                photoCollection.ShowsHorizontalScrollIndicator = false;
                photoCollection.RegisterClassForCell(typeof(PhotoGalleryCell), nameof(PhotoGalleryCell));
                var galleryCollectionViewSource = new PhotoGalleryViewSource(ImageAssets);
                photoCollection.Source = galleryCollectionViewSource;
                photoCollection.BackgroundColor = UIColor.White;
            }

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

            tagField = new UITextField();
            tagField.Font = Constants.Regular14;
            tagField.Placeholder = "Hashtag";
            tagField.AutocorrectionType = UITextAutocorrectionType.No;
            tagField.AutocapitalizationType = UITextAutocapitalizationType.None;

            hashtagImage = new UIImageView();
            hashtagImage.Image = UIImage.FromBundle("ic_hash");

            var hashtagCollectionSeparator = new UIView();
            hashtagCollectionSeparator.BackgroundColor = Constants.R245G245B245;

            postPhotoButton = new UIButton(); 
            postPhotoButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText), UIControlState.Normal);
            postPhotoButton.SetTitle("", UIControlState.Disabled);
            postPhotoButton.Layer.CornerRadius = 25;
            postPhotoButton.TitleLabel.Font = Constants.Semibold14;
            postPhotoButton.TouchDown += PostPhoto;
            //Constants.CreateGradient(postPhotoButton, 25);
            //postPhotoButton.BackgroundColor = UIColor.Blue;

            loadingView = new UIActivityIndicatorView();
            loadingView.Color = UIColor.White;
            loadingView.HidesWhenStopped = true;

            mainScroll.Bounces = false;
            //mainScroll.AddSubview(photoView);
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

            if (ImageAssets.Count == 1)
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoView, 30f);
            else
                photoTitleSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoCollection, 30f);
            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, _separatorMargin);
            photoTitleSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, _separatorMargin);
            photoTitleSeparator.AutoSetDimension(ALDimension.Height, 1f);

            titleTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, photoTitleSeparator, 25f);
            titleTextField.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator, -5f);

            titleEditImage.AutoSetDimensionsToSize(new CGSize(16, 16));
            titleEditImage.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            titleEditImage.AutoPinEdge(ALEdge.Left, ALEdge.Right, titleTextField, 5f);
            titleEditImage.AutoAlignAxis(ALAxis.Horizontal, titleTextField);

            titleDescriptionSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, titleTextField, 25f);
            titleDescriptionSeparator.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            titleDescriptionSeparator.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            titleDescriptionSeparator.AutoSetDimension(ALDimension.Height, 1f);

            descriptionTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, titleDescriptionSeparator, 25f);
            descriptionTextField.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator, -5f);
            descriptionTextField.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator, 5f);

            descriptionEditImage.AutoSetDimensionsToSize(new CGSize(16, 16));
            descriptionEditImage.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            descriptionEditImage.AutoPinEdge(ALEdge.Left, ALEdge.Right, descriptionTextField, 5f);
            descriptionEditImage.AutoAlignAxis(ALAxis.Horizontal, descriptionTextField);

            descriptionHashtagSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, descriptionTextField, 25f);
            descriptionHashtagSeparator.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            descriptionHashtagSeparator.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            descriptionHashtagSeparator.AutoSetDimension(ALDimension.Height, 1f);

            //toTop = tagField.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15f);
            //toTop.Active = false;

            norm = tagField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, descriptionHashtagSeparator, 25f);
            tagField.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);

            hashtagImage.AutoSetDimensionsToSize(new CGSize(14, 16));
            hashtagImage.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            hashtagImage.AutoPinEdge(ALEdge.Left, ALEdge.Right, tagField, 5f);
            hashtagImage.AutoAlignAxis(ALAxis.Horizontal, tagField);

            hashtagCollectionSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagField, 25f);
            hashtagCollectionSeparator.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            hashtagCollectionSeparator.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            hashtagCollectionSeparator.AutoSetDimension(ALDimension.Height, 1f);

            tagsCollectionView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, hashtagCollectionSeparator, 25f);
            tagsCollectionView.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            tagsCollectionView.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            //tagsCollectionView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 15f);
            t = tagsCollectionView.AutoSetDimension(ALDimension.Height, 0f);

            postPhotoButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagsCollectionView, 40f);
            postPhotoButton.AutoPinEdge(ALEdge.Left, ALEdge.Left, photoTitleSeparator);
            postPhotoButton.AutoPinEdge(ALEdge.Right, ALEdge.Right, photoTitleSeparator);
            postPhotoButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 35f);
            postPhotoButton.AutoSetDimension(ALDimension.Height, 50f);
            //t = tagsCollectionView.AutoSetDimension(ALDimension.Height, 0f);

            loadingView.AutoAlignAxis(ALAxis.Horizontal, postPhotoButton);
            loadingView.AutoAlignAxis(ALAxis.Vertical, postPhotoButton);

            //tagsTableView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, hashtagCollectionSeparator, 0f);
            //tagsTableView.AutoPinEdge(ALEdge.Left, ALEdge.Left, hashtagCollectionSeparator);
            //tagsTableView.AutoPinEdge(ALEdge.Right, ALEdge.Right, hashtagCollectionSeparator);
            //tableHeight = tagsTableView.AutoSetDimension(ALDimension.Height, 300f);
            //tagsTableView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 15f);
            //tagsTableView.BackgroundColor = UIColor.Black;
        }

        private NSLayoutConstraint tableHeight;
        private NSLayoutConstraint t;

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (_isFromCamera)
            {
                photoView.Image = await NormalizeImage(ImageAssets[0].Item2);
                RotatePhotoIfNeeded();
            }
        }

        private void SetPlaceholder()
        {
            var _titleTextViewDelegate = new PostTitleTextViewDelegate();
            titleTextField.Delegate = _titleTextViewDelegate;

            var titlePlaceholderLabel = new UILabel();
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

            var _descriptionTextViewDelegate = new PostTitleTextViewDelegate(2048);
            descriptionTextField.Delegate = _descriptionTextViewDelegate;

            var descriptionPlaceholderLabel = new UILabel();
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

            _descriptionTextViewDelegate.EditingStartedAction += EditingStartedAction;
            _titleTextViewDelegate.EditingStartedAction += EditingStartedAction;
        }

        private void EditingStartedAction()
        {
            AddOkButton();
        }

        private void EditingDidBegin(object sender, EventArgs e)
        {
            AddOkButton();
            AnimateView(true);
        }

        private void EditingDidChange(object sender, EventArgs e)
        {
            var txt = ((UITextField)sender).Text;
            if (!string.IsNullOrWhiteSpace(txt))
            {
                if (txt.EndsWith(" "))
                {
                    ((UITextField)sender).Text = string.Empty;
                    AddTag(txt);
                }
            }
            //_timer.Change(500, Timeout.Infinite);
        }

        private void EditingDidEnd(object sender, EventArgs e)
        {
            AnimateView(false);
        }
        /*
        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            tagsTableView.ContentInset = new UIEdgeInsets(0, 0, UIKeyboard.FrameEndFromNotification(notification).Height, 0);
            base.KeyBoardUpNotification(notification);
        }*/

        private void OnTimer(object state)
        {
            InvokeOnMainThread(() =>
            {
                SearchTextChanged();
            });
        }

        private void CollectionCellAction(ActionType type, string tag)
        {
            RemoveTag(tag);
        }

        private void TableCellAction(ActionType type, string tag)
        {
            AddTag(tag);
        }

        private async void SearchTextChanged()
        {
            if (_previousQuery == tagField.Text || tagField.Text.Length == 1)
                return;

            _previousQuery = tagField.Text;
            _presenter.Clear();

            ErrorBase error = null;
            if (tagField.Text.Length == 0)
                error = await _presenter.TryGetTopTags();
            else if (tagField.Text.Length > 1)
                error = await _presenter.TryLoadNext(tagField.Text);

            ShowAlert(error);
        }

        private void SourceChanged(Status obj)
        {
            //tagsTableView.ReloadData();
        }

        private void AnimateView(bool tagsOpened)
        {
            //norm.Active = !tagsOpened;
            //toTop.Active = tagsOpened;

            UIView.Animate(0.2, () =>
            {
                //photoView.Hidden = tagsOpened;
                titleEditImage.Hidden = tagsOpened;
                titleTextField.Hidden = tagsOpened;
                descriptionEditImage.Hidden = tagsOpened;
                descriptionTextField.Hidden = tagsOpened;
                //tagsTableView.Hidden = !tagsOpened;
                //titleBottomView.Hidden = tagsOpened;
                View.LayoutIfNeeded();
            });
        }

        private void AddTag(string txt)
        {
            if (!_collectionviewSource.LocalTags.Contains(txt))
            {
                //localTagsHeight.Constant = 50;
                //localTagsTopSpace.Constant = 15;
                _collectionviewSource.LocalTags.Add(txt);
                _collectionViewDelegate.GenerateVariables();
                tagsCollectionView.ReloadData();
                ResizeView();
                //tagsCollectionView.ScrollToItem(NSIndexPath.FromItemSection(_collectionviewSource.LocalTags.Count - 1, 0), UICollectionViewScrollPosition.Right, true);
            }
        }

        private void RemoveTag(string tag)
        {
            _collectionviewSource.LocalTags.Remove(tag);
            _collectionViewDelegate.GenerateVariables();
            tagsCollectionView.ReloadData();
            if (_collectionviewSource.LocalTags.Count == 0)
            {
                //localTagsHeight.Constant = 0;
                //localTagsTopSpace.Constant = 0;
            }
            ResizeView();
        }

        private bool _isinitialized;

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

        private void ResizeView()
        {
            //mainScroll.SetNeedsLayout();
            //mainScroll.LayoutIfNeeded();
            tagsCollectionView.LayoutIfNeeded();
            var collectionContentSize = tagsCollectionView.ContentSize;
            t.Constant = collectionContentSize.Height;
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostSettings);
            NavigationController.NavigationBar.Translucent = false;
        }

        private void AddOkButton()
        {
            var leftBarButton = new UIBarButtonItem("OK", UIBarButtonItemStyle.Plain, DoneTapped);
            NavigationItem.RightBarButtonItem = leftBarButton;
        }

        private void DoneTapped()
        {
            if (!string.IsNullOrEmpty(tagField.Text))
            {
                AddTag(tagField.Text);
                tagField.Text = string.Empty;
            }
            RemoveFocusFromTextFields();
        }

        private void RemoveFocusFromTextFields()
        {
            descriptionTextField.ResignFirstResponder();
            titleTextField.ResignFirstResponder();
            tagField.ResignFirstResponder();

            NavigationItem.RightBarButtonItem = null;
        }

        protected override void CreatePresenter()
        {
            _presenter = new PostDescriptionPresenter();
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

                var request = new UploadMediaModel(BasePresenter.User.UserInfo, stream, ImageExtension);
                return await _presenter.TryUploadMedia(request);
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new OperationResult<MediaModel>(new AppError(LocalizationKeys.PhotoProcessingError));
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

        private async Task CheckOnSpam()
        {
            _isSpammer = false;
            ToggleAvailability(false);

            try
            {
                var username = BasePresenter.User.Login;
                var spamCheck = await _presenter.TryCheckForSpam(username);

                if (!spamCheck.IsSuccess)
                    return;

                if (!spamCheck.Result.IsSpam)
                {
                    if (spamCheck.Result.WaitingTime > 0)
                    {
                        _isSpammer = true;
                        StartPostTimer((int)spamCheck.Result.WaitingTime);
                    }
                }
                else
                {
                    // more than 15 posts
                    // TODO: need to show alert
                    _isSpammer = true;
                }
            }
            finally
            {
                ToggleAvailability(true);
            }
        }

        private async void StartPostTimer(int startSeconds)
        {
            var timepassed = PostingLimit - TimeSpan.FromSeconds(startSeconds);
            postPhotoButton.UserInteractionEnabled = false;

            while (timepassed < PostingLimit)
            {
                UIView.PerformWithoutAnimation(() =>
                {
                    postPhotoButton.SetTitle((PostingLimit - timepassed).ToString("mm\\:ss"), UIControlState.Normal);
                    postPhotoButton.LayoutIfNeeded();
                });
                await Task.Delay(1000);

                timepassed = timepassed.Add(TimeSpan.FromSeconds(1));
            }

            _isSpammer = false;
            postPhotoButton.UserInteractionEnabled = true;
            postPhotoButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText), UIControlState.Normal);
        }

        private async void PostPhoto(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(titleTextField.Text))
            {
                ShowAlert(LocalizationKeys.EmptyTitleField);
                return;
            }

            await CheckOnSpam();

            if (_isSpammer)
                return;

            ToggleAvailability(false);

            await Task.Run(() =>
            {
                try
                {
                    string title = null;
                    string description = null;
                    IList<string> tags = null;

                    InvokeOnMainThread(() =>
                    {
                        title = titleTextField.Text;
                        description = descriptionTextField.Text;
                        tags = _collectionviewSource.LocalTags;
                    });

                    var mre = new ManualResetEvent(false);

                    var shouldReturn = false;
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
                                ShowDialog(photoUploadResponse[0].Error, LocalizationKeys.Cancel, LocalizationKeys.Retry, (arg) =>
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

                    var model = new PreparePostModel(BasePresenter.User.UserInfo)
                    {
                        Title = title,
                        Description = description,

                        Tags = tags.ToArray(),
                        Media = photoUploadResponse.Select(r => r.Result).ToArray(),
                    };

                    var pushToBlockchainRetry = false;
                    do
                    {
                        pushToBlockchainRetry = false;
                        var response = _presenter.TryCreateOrEditPost(model).Result;
                        if (!(response != null && response.IsSuccess))
                        {
                            InvokeOnMainThread(() =>
                            {
                                ShowDialog(response.Error, LocalizationKeys.Cancel, LocalizationKeys.Retry, (arg) =>
                                 {
                                     mre.Set();
                                 }, (arg) =>
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
                                NavigationController.ViewControllers = new UIViewController[] { NavigationController.ViewControllers[0], this };
                                NavigationController.PopViewController(false);
                            });
                        }
                    } while (pushToBlockchainRetry);
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        ToggleAvailability(true);
                    });
                }
            });
        }

        private async Task<UIImage> NormalizeImage(UIImage sourceImage)
        {
            return await Task.Run(() =>
            {
                var imgSize = sourceImage.Size;
                var inSampleSize = ImageHelper.CalculateInSampleSize(sourceImage.Size, 1200, 1200);
                UIGraphics.BeginImageContextWithOptions(inSampleSize, false, sourceImage.CurrentScale);

                var drawRect = new CGRect(0, 0, inSampleSize.Width, inSampleSize.Height);
                sourceImage.Draw(drawRect);
                var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                return modifiedImage;
            });
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
            photoView.Image = ImageHelper.RotateImage(photoView.Image, orientation);
        }

        private void ToggleAvailability(bool enabled)
        {
            if (enabled)
                loadingView.StopAnimating();
            else
                loadingView.StartAnimating();

            postPhotoButton.Enabled = enabled;
            titleTextField.UserInteractionEnabled = enabled;
            descriptionTextField.UserInteractionEnabled = enabled;
            tagField.Enabled = enabled;
            tagsCollectionView.UserInteractionEnabled = enabled;
        }

        private void GoBack(object sender, EventArgs e)
        {
            //if (tagToTop.Active)
            //RemoveFocusFromTextFields();
            //else
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
