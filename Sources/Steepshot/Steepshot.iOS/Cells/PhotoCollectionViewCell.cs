using System;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Photos;
using PureLayout.Net;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class PhotoCollectionViewCell : UICollectionViewCell
    {
        private string ImageUrl;
        private IScheduledWork _scheduledWork;
        private Post _currentPost;
        private UIImageView _bodyImage;
        private UIView _selectFrame;
        private UIView _selectView;
        private UILabel _countLabel;

        public bool IsSelected
        {
            get;
            set;
        }

        protected PhotoCollectionViewCell(IntPtr handle) : base(handle)
        {
        }

        public void UpdateImage(PHImageManager cm, PHAsset photo, bool isCurrentlySelected, int count = 0, bool? isSelected = null)
        {
            if (_bodyImage == null)
                CreateImageView();

            if (_selectView == null)
            {
                _selectView = new UIView(new CGRect(ContentView.Frame.Right - 38, 8, 30, 30));
                _selectView.Layer.BorderColor = UIColor.White.CGColor;
                _selectView.Layer.BorderWidth = 2;
                _selectView.Layer.CornerRadius = 15;
                _selectView.BackgroundColor = UIColor.Clear;
                _selectView.Hidden = true;
                _selectView.ClipsToBounds = true;
                ContentView.AddSubview(_selectView);
            }

            if (_countLabel == null)
            {
                _countLabel = new UILabel();
                _countLabel.Font = Constants.Semibold16;
                _countLabel.TextColor = UIColor.White;
                _selectView.AddSubview(_countLabel);
                _countLabel.AutoCenterInSuperview();

            }
            _countLabel.Text = (count + 1).ToString();
            

            if (_selectFrame == null)
            {
                _selectFrame = new UIView(ContentView.Frame);
                _selectFrame.Layer.BorderColor = Constants.R255G81B4.CGColor;
                _selectFrame.Layer.BorderWidth = 3;
                _selectFrame.BackgroundColor = UIColor.Clear;
               
                ContentView.AddSubview(_selectFrame);
            }
            _selectFrame.Hidden = !isCurrentlySelected;

            ManageSelector(isSelected);

            cm.RequestImageForAsset(photo, new CGSize(200, 200),
                                                 PHImageContentMode.AspectFill, new PHImageRequestOptions() { Synchronous = true }, (img, info) =>
                                       {
                                           _bodyImage.Image = img;
                                       });
        }

        private void ManageSelector(bool? isSelected)
        {
            switch (isSelected)
            {
                case true:
                    _selectView.Hidden = false;
                    _selectView.Layer.BorderColor = Constants.R255G81B4.CGColor;
                    _selectView.BackgroundColor = Constants.R255G81B4;
                    _countLabel.TextColor = UIColor.White;
                    break;
                case false:
                    _selectView.Hidden = false;
                    _selectView.Layer.BorderColor = UIColor.White.CGColor;
                    _selectView.BackgroundColor = UIColor.Clear;
                    _countLabel.TextColor = UIColor.Clear;
                    break;
                case null:
                    _selectView.Hidden = true;
                    break;
            }
        }

        public void ToggleCell(bool isCurrentSelection)
        {
            _selectFrame.Hidden = !isCurrentSelection;
        }

        public void UpdateCell(Post post)
        {
            _currentPost = post;

            ImageUrl = post.Media[0].Thumbnails.Micro;

            _bodyImage?.RemoveFromSuperview();
            CreateImageView();

            _scheduledWork?.Cancel();
            _scheduledWork = ImageService.Instance.LoadUrl(ImageUrl)
                                         .Retry(2)
                                         .FadeAnimation(false)
                                         .WithCache(FFImageLoading.Cache.CacheType.All)
                                         .WithPriority(LoadingPriority.Highest)
                                         .DownSample(250)
                                          /* .DownloadProgress((f)=>
                                         {
                                         })*/
                                          .Into(_bodyImage);
        }

        private void CreateImageView()
        {
            _bodyImage = new UIImageView();
            _bodyImage.ClipsToBounds = true;
            _bodyImage.UserInteractionEnabled = true;
            _bodyImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            _bodyImage.Frame = new CGRect(new CGPoint(0, 0), Constants.CellSize);
            _bodyImage.BackgroundColor = UIColor.FromRGB(244, 244, 246);

            ContentView.AddSubview(_bodyImage);
        }
    }
}
