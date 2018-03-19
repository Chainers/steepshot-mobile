using System;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Photos;
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

        protected PhotoCollectionViewCell(IntPtr handle) : base(handle)
        {
        }

        public void UpdateImage(PHImageManager cm, PHAsset photo)
        {
            if (_bodyImage == null)
                CreateImageView();

            cm.RequestImageForAsset(photo, new CGSize(200, 200),
                                                 PHImageContentMode.AspectFill, new PHImageRequestOptions() { Synchronous = true }, (img, info) =>
                                       {
                                           _bodyImage.Image = img;
                                       });
        }

        public void UpdateCell(Post post)
        {
            _currentPost = post;

            var thumbnail = post.Media[0].Thumbnails?[256];
            ImageUrl = string.IsNullOrEmpty(thumbnail) ? post.Media[0].Url : thumbnail;

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
