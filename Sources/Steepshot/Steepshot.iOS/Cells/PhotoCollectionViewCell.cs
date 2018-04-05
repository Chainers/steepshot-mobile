using System;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
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
        private UIImageView _galleryImage;

        protected PhotoCollectionViewCell(IntPtr handle) : base(handle)
        {
            _galleryImage = new UIImageView(new CGRect(Constants.CellSideSize - 15, 5, 10, 10));
            _galleryImage.Image = UIImage.FromBundle("ic_is_gallery");
            ContentView.AddSubview(_galleryImage);
        }

        public void UpdateCell(Post post)
        {
            _currentPost = post;

            ImageUrl = post.Media[0].Thumbnails.Micro;

            _bodyImage?.RemoveFromSuperview();
            _bodyImage = new UIImageView();
            _bodyImage.ClipsToBounds = true;
            _bodyImage.UserInteractionEnabled = true;
            _bodyImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            _bodyImage.Frame = new CGRect(new CGPoint(0, 0), Constants.CellSize);
            _bodyImage.BackgroundColor = UIColor.FromRGB(244, 244, 246);

            ContentView.AddSubview(_bodyImage);

            _scheduledWork?.Cancel();
            _scheduledWork = ImageService.Instance.LoadUrl(ImageUrl)
                                         .Retry(2)
                                         .FadeAnimation(false)
                                         .WithCache(FFImageLoading.Cache.CacheType.All)
                                         .WithPriority(LoadingPriority.Highest)
                                         .DownSample(300)
                                          /* .DownloadProgress((f)=>
                                         {
                                         })*/
                                          .Into(_bodyImage);
            if (post.Media.Length > 1)
                ContentView.BringSubviewToFront(_galleryImage);
        }
    }
}
