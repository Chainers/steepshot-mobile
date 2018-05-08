using System;
using FFImageLoading;
using PureLayout.Net;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class PhotoGalleryCell : UICollectionViewCell
    {
        private UIImageView _bodyImage;

        protected PhotoGalleryCell(IntPtr handle) : base(handle)
        {
            _bodyImage = new UIImageView();
            _bodyImage.ClipsToBounds = true;
            _bodyImage.ContentMode = UIViewContentMode.ScaleAspectFill;
            _bodyImage.Layer.CornerRadius = 8;

            ContentView.AddSubview(_bodyImage);

            _bodyImage.AutoPinEdgesToSuperviewEdges();
        }

        public void UpdateImage(UIImage image)
        {
            _bodyImage.Image = image;
        }

        public void UpdateImage(string url)
        {
            ImageService.Instance.LoadUrl(url).Into(_bodyImage);
        }
    }
}
