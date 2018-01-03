using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
using Steepshot.Core;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Models;

namespace Steepshot.iOS.Views
{
    public partial class DescriptionViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        public UIImage ImageAsset;
        private TagsTableViewSource _tableSource;
        private LocalTagsCollectionViewSource _collectionviewSource;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            postPhotoButton.Layer.CornerRadius = 25;
            Constants.CreateShadow(postPhotoButton, Constants.R231G72B0, 0.5f, 25, 10, 12);

            postPhotoButton.TitleLabel.Font = Constants.Semibold14;
            tagField.Font = titleTextField.Font = descriptionTextField.Font = Constants.Regular14;

            //titleTextField.ContentMode = UIViewContentMode.Center;
            //titleTextField.TextAlignment = UITextAlignment.Center;

            //descriptionTextField.TextAlignment = UITextAlignment.Center;

            _tableSource = new TagsTableViewSource();
            //_tableSource.CellAction += CellAction;

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
            tagsCollectionView.Source = _collectionviewSource;

            _collectionviewSource.CellAction += (ActionType arg1, string arg2) => 
            {
                RemoveTag(arg2);
            };

            tagField.EditingChanged += (object sender, EventArgs e) =>
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
            };

            tagField.EditingDidBegin += (object sender, EventArgs e) =>
            {
                View.LayoutIfNeeded();
                UIView.Animate(0.2, () =>
                {
                    tagDefault.Active = false;
                    tagToTop.Active = true;

                    photoView.Hidden = true;
                    titleEditImage.Hidden = true;
                    titleTextField.Hidden = true;
                    tagsTableView.Hidden = false;
                    titleBottomView.Hidden = true;

                    View.LayoutIfNeeded();
                });
            };

            tagField.EditingDidEnd += (object sender, EventArgs e) =>
            {
                View.LayoutIfNeeded();
                UIView.Animate(0.2, () =>
                {
                    tagDefault.Active = true;
                    tagToTop.Active = false;

                    photoView.Hidden = false;
                    titleEditImage.Hidden = false;
                    titleTextField.Hidden = false;
                    tagsTableView.Hidden = true;
                    titleBottomView.Hidden = false;

                    View.LayoutIfNeeded();
                });
            };

            /*
            _collectionviewSource = new TagsCollectionViewSource((sender, e) =>
            {
                var myViewController = new PostTagsViewController();
                NavigationController.PushViewController(myViewController, true);
            });*/
            //_collectionviewSource.TagsCollection = new List<string>() { "" }; //BaseViewController.User.TagsList;
            //_collectionviewSource.RowSelectedEvent += CollectionTagSelected;
            //tagsCollectionView.Source = _collectionviewSource;

            var tap = new UITapGestureRecognizer(RemoveFocusFromTextFields);
            View.AddGestureRecognizer(tap);

            //postPhotoButton.TouchDown += (sender, e) => PostPhoto();

            postPhotoButton.TouchDown += (sender, e) =>
            {
                
            };
            Activeview = descriptionTextField;

            SetBackButton();
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            tagsTableView.ContentInset = new UIEdgeInsets(0, 0, UIKeyboard.FrameEndFromNotification(notification).Height, 0);
            base.KeyBoardUpNotification(notification);
        }

        private void AddTag(string txt)
        {
            localTagsHeight.Constant = 50;
            localTagsTopSpace.Constant = 15;

            _collectionviewSource.LocalTags.Add(txt);
            tagsCollectionView.ReloadData();
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

        /*
        private void RemoveTag(string txt)
        {
           
            localTagsHeight.Constant = 50;
            localTagsTopSpace.Constant = 15;

            _collectionviewSource.LocalTags.Add(txt);
            tagsCollectionView.ReloadData();
            tagsCollectionView.CollectionViewLayout.InvalidateLayout();
        }*/

        public override void ViewDidLayoutSubviews()
        {
            Constants.CreateGradient(postPhotoButton, 25);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = Localization.Messages.PostSettings;
            NavigationController.NavigationBar.Translucent = false;
        }

        private void RemoveFocusFromTextFields()
        {
            descriptionTextField.ResignFirstResponder();
            titleTextField.ResignFirstResponder();
            tagField.ResignFirstResponder();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            photoView.Image = await NormalizeImage(ImageAsset);
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

        private async void PostPhoto()
        {
            //loadingView.Hidden = false;
            postPhotoButton.Enabled = false;

            try
            {
                byte[] photoByteArray;
                using (NSData imageData = photoView.Image.AsJPEG(0.9f))
                {
                    photoByteArray = new Byte[imageData.Length];
                    Marshal.Copy(imageData.Bytes, photoByteArray, 0, Convert.ToInt32(imageData.Length));
                }

                var request = new UploadImageRequest(BasePresenter.User.UserInfo, titleTextField.Text, photoByteArray, TagsList.ToArray())
                {
                    Description = descriptionTextField.Text
                };
                var serverResult = await _presenter.TryUploadWithPrepare(request);
                if (!serverResult.Success)
                {
                    ShowAlert(serverResult);
                }
                else
                {
                    var result = await _presenter.TryUpload(request, serverResult.Result);

                    if (result != null && result.Success)
                    {
                        TagsList.Clear();
                        ShouldProfileUpdate = true;
                        NavigationController.PopViewController(true);
                    }
                    else
                    {
                        ShowAlert(result);
                    }
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                //loadingView.Hidden = true;
                postPhotoButton.Enabled = true;
            }
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        /*
        protected override void CalculateBottom()
        {
            Bottom = Activeview.Frame.Y + scrollView.Frame.Y - scrollView.ContentOffset.Y + Activeview.Frame.Height + Offset;
        }*/
    }

    /*
    public class Del : UICollectionViewDelegate
    {
        override DidSelec
    }*/
}
