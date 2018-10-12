using System;
using CoreGraphics;
using FFImageLoading.Work;
using Photos;
using PureLayout.Net;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class PhotoCollectionViewCell : UICollectionViewCell
    {
        private IScheduledWork _scheduledWork;
        private Post _currentPost;
        private UIImageView _bodyImage;
        private UIImageView _galleryImage;
        private UIView _selectFrame;
        private UIView _selectView;
        private UILabel _countLabel;
        private readonly TriangleView _triangle = new TriangleView();

        public bool IsSelected
        {
            get;
            set;
        }

        protected PhotoCollectionViewCell(IntPtr handle) : base(handle)
        {
            _galleryImage = new UIImageView(new CGRect(Constants.CellSideSize - 15, 5, 10, 10));
            _galleryImage.Image = UIImage.FromBundle("ic_is_gallery");
            ContentView.AddSubview(_galleryImage);

            ContentView.AddSubview(_triangle);
            _triangle.AutoSetDimensionsToSize(new CGSize(10, 10));
            _triangle.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 5);
            _triangle.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 5);
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

            _bodyImage?.RemoveFromSuperview();
            CreateImageView();

            _scheduledWork?.Cancel();
            if (_currentPost.Media[0].ContentType == MimeTypeHelper.GetMimeType(MimeTypeHelper.Mp4))
            {
                _scheduledWork = ImageLoader.Load(_currentPost.Media[0].Thumbnails.Micro,
                                                     _bodyImage, 2,
                                                     LoadingPriority.Highest);
            }
            else
            {
                _scheduledWork = ImageLoader.Load(_currentPost.Media[0].Url,
                                                      _bodyImage, 2,
                                                      LoadingPriority.Highest, microUrl: _currentPost.Media[0].Thumbnails.Micro);
            }
            if (_currentPost.Media.Length > 1)
                ContentView.BringSubviewToFront(_galleryImage);
            if(MimeTypeHelper.IsVideo(_currentPost.Media[0].ContentType))
                ContentView.BringSubviewToFront(_triangle);
        }

        public void UpdateCell(UIImage image)
        {
            _bodyImage?.RemoveFromSuperview();
            CreateImageView();
            _bodyImage.Image = image;
        }

        private void CreateImageView()
        {
            _bodyImage = new UIImageView();
            _bodyImage.ClipsToBounds = true;
            _bodyImage.UserInteractionEnabled = true;
            _bodyImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            _bodyImage.Frame = new CGRect(new CGPoint(0, 0), Constants.CellSize);
            _bodyImage.BackgroundColor = Constants.R244G244B246;

            ContentView.AddSubview(_bodyImage);
        }
    }
}
