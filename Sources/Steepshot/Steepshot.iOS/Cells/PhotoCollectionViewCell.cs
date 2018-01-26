using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Photos;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class PhotoCollectionViewCell : BaseProfileCell
    {
        protected PhotoCollectionViewCell(IntPtr handle) : base(handle) { }
        public static readonly NSString Key = new NSString(nameof(PhotoCollectionViewCell));
        public static readonly UINib Nib;
        public PHAsset Asset;
        public UIImage Image => photoImg.Image;
        public string ImageUrl;
        private IScheduledWork _scheduledWork;
        private readonly int _downSampleWidth = (int)Constants.CellSize.Width;

        bool isInitialized;

        static PhotoCollectionViewCell()
        {
            Nib = UINib.FromName(nameof(PhotoCollectionViewCell), NSBundle.MainBundle);
        }

        public void UpdateImage(UIImage photo, PHAsset asset)
        {
            photoImg.Image = photo;
            Asset = asset;
        }

        public override void UpdateCell(Post post)
        {
            if (!isInitialized)
            {
                widthConstraint.Constant = heightConstraint.Constant = Constants.CellSideSize;
                isInitialized = true;
            }

            ImageUrl = post.Media[0].Url;
            photoImg.Image = null;
            _scheduledWork?.Cancel();
            if (ImageUrl != null)
                _scheduledWork = ImageService.Instance.LoadUrl(ImageUrl, Constants.ImageCacheDuration)
                                             .FadeAnimation(false)
                                             .DownSample(width: _downSampleWidth)
                                             .Into(photoImg);
        }
    }
}
