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

namespace Steepshot.iOS.Views
{
    public partial class DescriptionViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new PostDescriptionPresenter();
        }
        public UIImage ImageAsset;

        private TagsCollectionViewSource _collectionviewSource;

        public override void ViewDidLoad()
        {
            SetNavBar();
            base.ViewDidLoad();
            SwitchDescription();
            photoView.Image = NormalizeImage(ImageAsset);
            postPhotoButton.TouchDown += (sender, e) => PostPhoto();
            Activeview = descriptionTextField;
            //Collection view initialization
            tagsCollectionView.RegisterClassForCell(typeof(TagCollectionViewCell), nameof(TagCollectionViewCell));
            tagsCollectionView.RegisterNibForCell(UINib.FromName(nameof(TagCollectionViewCell), NSBundle.MainBundle), nameof(TagCollectionViewCell));
            // research flow layout
            /*tagsCollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                EstimatedItemSize = new CGSize(100, 50),
                
            }, false);*/
            _collectionviewSource = new TagsCollectionViewSource((sender, e) =>
            {
                var myViewController = new PostTagsViewController();
                NavigationController.PushViewController(myViewController, true);
            });
            _collectionviewSource.TagsCollection = new List<string>() { "" }; //BaseViewController.User.TagsList;
            _collectionviewSource.RowSelectedEvent += CollectionTagSelected;
            tagsCollectionView.Source = _collectionviewSource;

            UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
                {
                    descriptionTextField.ResignFirstResponder();
                });
            View.AddGestureRecognizer(tap);
            titleTextField.Layer.BorderWidth = descriptionTextField.Layer.BorderWidth = 1;
            titleTextField.Layer.BorderColor = descriptionTextField.Layer.BorderColor = UIColor.Black.CGColor;
        }

        private void SetNavBar()
        {
            NavigationController.SetNavigationBarHidden(false, false);
            var barHeight = NavigationController.NavigationBar.Frame.Height;

            var tw = new UILabel(new CGRect(0, 0, 120, barHeight));
            tw.TextColor = UIColor.White;
            tw.BackgroundColor = UIColor.Clear;
            tw.TextAlignment = UITextAlignment.Center;
            tw.Font = UIFont.SystemFontOfSize(17);

            NavigationItem.TitleView = tw;

            var button = new UIButton();
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                button.WidthAnchor.ConstraintEqualTo(32).Active = true;
                button.HeightAnchor.ConstraintEqualTo(32).Active = true;
            }
            else
            {
                button.Frame = new CGRect(0, 0, 32, 32);
            }
            button.Layer.BorderColor = UIColor.White.CGColor;
            button.Layer.BorderWidth = 2.0f;
            button.Layer.CornerRadius = 16;
            button.SetTitle("+", UIControlState.Normal);
            button.TitleLabel.Font = UIFont.SystemFontOfSize(25);
            button.TitleEdgeInsets = new UIEdgeInsets(0, 0, 4, 0);
            button.TouchUpInside += AddDescriptionButtonClick;
            var rightBarButton = new UIBarButtonItem();
            rightBarButton.CustomView = button;
            NavigationItem.SetRightBarButtonItem(rightBarButton, true);

            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB(66, 165, 245); // To constants
        }

        void SwitchDescription()
        {
            descriptionLabel.Hidden = !descriptionLabel.Hidden;
            descriptionTextField.Hidden = !descriptionTextField.Hidden;
            tagsCollectionVerticalSpacing.Active = !descriptionTextField.Hidden;
            tagsCollectionVerticalSpacingHidden.Active = descriptionTextField.Hidden;
            tagsCollectionView.SetNeedsLayout();
        }

        void AddDescriptionButtonClick(object sender, EventArgs e)
        {
            SwitchDescription();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (NavigationController != null)
                NavigationController.NavigationBarHidden = false;
            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _collectionviewSource.TagsCollection.Clear();
            _collectionviewSource.TagsCollection.Add("");
            _collectionviewSource.TagsCollection.AddRange(TagsList);
            tagsCollectionView.ReloadData();
            tagsCollectionView.LayoutIfNeeded();
            collectionHeight.Constant = tagsCollectionView.ContentSize.Height;
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
                TagsList.Clear();
            base.ViewDidDisappear(animated);
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

        private UIImage NormalizeImage(UIImage sourceImage)
        {
            var imgSize = sourceImage.Size;
            var inSampleSize = CalculateInSampleSize(sourceImage, 1200, 1200);
            UIGraphics.BeginImageContextWithOptions(inSampleSize, false, sourceImage.CurrentScale);

            var drawRect = new CGRect(0, 0, inSampleSize.Width, inSampleSize.Height);
            sourceImage.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return modifiedImage;
        }

        private async void PostPhoto()
        {
            loadingView.Hidden = false;
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
                var result = await _presenter.TryUpload(request);

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
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                loadingView.Hidden = true;
                postPhotoButton.Enabled = true;
            }
        }

        void CollectionTagSelected(int row)
        {
            _collectionviewSource.TagsCollection.RemoveAt(row);
            TagsList.RemoveAt(row - 1);
            tagsCollectionView.ReloadData();
        }

        protected override void CalculateBottom()
        {
            Bottom = Activeview.Frame.Y + scrollView.Frame.Y - scrollView.ContentOffset.Y + Activeview.Frame.Height + Offset;
        }
    }
}
