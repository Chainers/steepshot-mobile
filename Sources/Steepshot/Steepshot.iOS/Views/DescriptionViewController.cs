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
using FFImageLoading.Extensions;
using System.Threading.Tasks;
using Steepshot.Core;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Models;
using System.Threading;
using Steepshot.Core.Errors;
using Steepshot.iOS.Helpers;
using Steepshot.Core.Models.Common;
using System.Collections.Generic;
using Steepshot.Core.Models.Enums;
using System.IO;
using System.Linq;

namespace Steepshot.iOS.Views
{
    public partial class DescriptionViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        private UIImage ImageAsset;
        private string ImageExtension;
        private TagsTableViewSource _tableSource;
        private LocalTagsCollectionViewSource _collectionviewSource;
        private Timer _timer;
        private string _previousQuery;

        public DescriptionViewController(UIImage imageAsset, string extension)
        {
            ImageAsset = imageAsset;
            ImageExtension = extension;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Activeview = descriptionTextField;
            postPhotoButton.Layer.CornerRadius = 25;
            postPhotoButton.TitleLabel.Font = Constants.Semibold14;
            tagField.Font = titleTextField.Font = descriptionTextField.Font = Constants.Regular14;

            _tableSource = new TagsTableViewSource(_presenter);
            _tableSource.CellAction += TableCellAction;
            tagsTableView.Source = _tableSource;
            tagsTableView.LayoutMargins = UIEdgeInsets.Zero;
            tagsTableView.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTableView.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTableView.RowHeight = 65f;

            tagsCollectionView.RegisterClassForCell(typeof(LocalTagCollectionViewCell), nameof(LocalTagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(LocalTagCollectionViewCell), NSBundle.MainBundle), nameof(LocalTagCollectionViewCell));

            tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                EstimatedItemSize = new CGSize(20, 45),
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                SectionInset = new UIEdgeInsets(0, 15, 0, 0),
            }, false);

            _collectionviewSource = new LocalTagsCollectionViewSource();
            _collectionviewSource.CellAction += CollectionCellAction;
            tagsCollectionView.Source = _collectionviewSource;

            tagField.Delegate = new TagFieldDelegate(DoneTapped);
            tagField.EditingChanged += EditingDidChange;
            tagField.EditingDidBegin += EditingDidBegin;
            tagField.EditingDidEnd += EditingDidEnd;

            var tap = new UITapGestureRecognizer(RemoveFocusFromTextFields);
            View.AddGestureRecognizer(tap);

            var rotateTap = new UITapGestureRecognizer(RotateImage);
            rotateImage.AddGestureRecognizer(rotateTap);

            postPhotoButton.TouchDown += PostPhoto;

            _presenter.SourceChanged += SourceChanged;
            _timer = new Timer(OnTimer);

            SetBackButton();
            SearchTextChanged();
            SetPlaceholder();
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

            titlePlaceholderLabel.Frame = new CGRect(labelX, labelY, labelWidth, labelHeight);

            titleTextField.AddSubview(titlePlaceholderLabel);
            _titleTextViewDelegate.Placeholder = titlePlaceholderLabel;


            var _descriptionTextViewDelegate = new PostTitleTextViewDelegate();
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

            descriptionPlaceholderLabel.Frame = new CGRect(descLabelX, descLabelY, descLabelWidth, descLabelHeight);

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
            _timer.Change(500, Timeout.Infinite);
        }

        private void EditingDidEnd(object sender, EventArgs e)
        {
            AnimateView(false);
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            tagsTableView.ContentInset = new UIEdgeInsets(0, 0, UIKeyboard.FrameEndFromNotification(notification).Height, 0);
            base.KeyBoardUpNotification(notification);
        }

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
            tagsTableView.ReloadData();
        }

        private void AnimateView(bool tagsOpened)
        {
            View.LayoutIfNeeded();
            UIView.Animate(0.2, () =>
            {
                tagDefault.Active = !tagsOpened;
                tagToTop.Active = tagsOpened;

                rotateImage.Hidden = tagsOpened;
                photoView.Hidden = tagsOpened;
                titleEditImage.Hidden = tagsOpened;
                titleTextField.Hidden = tagsOpened;
                tagsTableView.Hidden = !tagsOpened;
                titleBottomView.Hidden = tagsOpened;

                View.LayoutIfNeeded();
            });
        }

        private void AddTag(string txt)
        {
            if (!_collectionviewSource.LocalTags.Contains(txt))
            {
                localTagsHeight.Constant = 50;
                localTagsTopSpace.Constant = 15;
                _collectionviewSource.LocalTags.Add(txt);
                tagsCollectionView.ReloadData();
            }

            //tagsCollectionView.CollectionViewLayout.InvalidateLayout();

            //await Task.Delay(100);

            //InvokeOnMainThread(() => {
            //tagsCollectionView.ScrollToItem(NSIndexPath.FromItemSection(_collectionviewSource.LocalTags.Count - 1, 0), UICollectionViewScrollPosition.Right, true);
            // });
        }

        private void RemoveTag(string tag)
        {
            _collectionviewSource.LocalTags.Remove(tag);
            tagsCollectionView.ReloadData();
            if (_collectionviewSource.LocalTags.Count == 0)
            {
                localTagsHeight.Constant = 0;
                localTagsTopSpace.Constant = 0;
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            Constants.CreateGradient(postPhotoButton, 25);
            Constants.CreateShadow(postPhotoButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = Localization.Messages.PostSettings;
            NavigationController.NavigationBar.Translucent = false;
        }

        private void AddOkButton()
        {
            var leftBarButton = new UIBarButtonItem("OK", UIBarButtonItemStyle.Plain, RemoveFocus);
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

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            ImageAsset = photoView.Image = await NormalizeImage(ImageAsset);
        }

        protected override void CreatePresenter()
        {
            _presenter = new PostDescriptionPresenter();
        }

        private CGSize CalculateInSampleSize(UIImage sourceImage, int reqWidth, int reqHeight)
        {
            var height = sourceImage.Size.Height;
            var width = sourceImage.Size.Width;
            var inSampleSize = 1.0;
            if (height > reqHeight)
            {
                inSampleSize = reqHeight / height;
            }
            if (width > reqWidth)
            {
                inSampleSize = Math.Min(inSampleSize, reqWidth / width);
            }

            return new CGSize(width * inSampleSize, height * inSampleSize);
        }

        private async Task<UIImage> NormalizeImage(UIImage sourceImage)
        {
            return await Task.Run(() =>
            {
                var imgSize = sourceImage.Size;
                var inSampleSize = CalculateInSampleSize(sourceImage, 1200, 1200);
                UIGraphics.BeginImageContextWithOptions(inSampleSize, false, sourceImage.CurrentScale);

                var drawRect = new CGRect(0, 0, inSampleSize.Width, inSampleSize.Height);
                sourceImage.Draw(drawRect);
                var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                return modifiedImage;
            });
        }

        private void RotateImage()
        {
            UIImageOrientation orientation;
            switch (photoView.Image.Orientation)
            {
                case UIImageOrientation.Up:
                    orientation = UIImageOrientation.Right;
                    break;
                case UIImageOrientation.Right:
                    orientation = UIImageOrientation.Down;
                    break;
                case UIImageOrientation.Down:
                    orientation = UIImageOrientation.Left;
                    break;
                case UIImageOrientation.Left:
                    orientation = UIImageOrientation.Up;
                    break;
                default:
                    orientation = UIImageOrientation.Up;
                    break;
            }
            ImageAsset = photoView.Image = new UIImage(photoView.Image.CGImage, 1, orientation);
        }

        private async Task<OperationResult<MediaModel>> UploadPhoto()
        {
            Stream stream = null;
            try
            {
                stream = ImageAsset.AsJpegStream();
                var request = new UploadMediaModel(BasePresenter.User.UserInfo, stream, ImageExtension);
                return await _presenter.TryUploadMedia(request);
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new OperationResult<MediaModel>(new ApplicationError(Localization.Errors.PhotoProcessingError));
            }
            finally
            {
                stream?.Flush();
                stream?.Dispose();
            }
        }

        private async void PostPhoto(object sender, EventArgs e)
        {
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
                    OperationResult<MediaModel> photoUploadResponse;
                    do
                    {
                        photoUploadRetry = false;
                        photoUploadResponse = UploadPhoto().Result;

                        if (!photoUploadResponse.IsSuccess)
                        {
                            InvokeOnMainThread(() =>
                            {
                                ShowDialog(photoUploadResponse.Error.Message, "Cancel", "Retry", (arg) =>
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
                        Media = new[] { photoUploadResponse.Result }
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
                                ShowDialog(response.Error.Message, "Cancel", "Retry", (arg) =>
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
            if (tagToTop.Active)
                RemoveFocusFromTextFields();
            else
                NavigationController.PopViewController(true);
        }

        private void RemoveFocus(object sender, EventArgs e)
        {
            RemoveFocusFromTextFields();
        }
    }
}
