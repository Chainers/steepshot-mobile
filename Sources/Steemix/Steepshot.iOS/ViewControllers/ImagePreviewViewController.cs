using System;
using CoreGraphics;
using FFImageLoading;
using UIKit;

namespace Steepshot.iOS
{
    public partial class ImagePreviewViewController : UIViewController
    {
        protected ImagePreviewViewController(IntPtr handle) : base(handle) { }
        public UIImage ImageForPreview;
        public string ImageUrl;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var margin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            var imageScrollView = new UIScrollView(new CGRect(0, margin, View.Frame.Width, View.Frame.Height - margin));
            View.AddSubview(imageScrollView);
            var imageView = new UIImageView(new CGRect(0, 0, imageScrollView.Frame.Width, imageScrollView.Frame.Height));
            if (ImageForPreview != null)
            {
                imageView.Image = ImageForPreview;
                imageScrollView.MinimumZoomScale = 1f;
                imageScrollView.MaximumZoomScale = 6.0f;
                imageScrollView.ViewForZoomingInScrollView += sv => imageView;
            }
            else
            {
                backgroundView.BackgroundColor = UIColor.White;
                imageView.Image = UIImage.FromBundle("ic_photo_holder");
            }
            ImageService.Instance.LoadUrl(ImageUrl, Constants.ImageCacheDuration)
                                                     .Retry(2, 200)
                                                     .Into(imageView);
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            imageScrollView.ContentSize = imageView.Image.Size;
            imageScrollView.AddSubview(imageView);
            imageScrollView.ContentSize = new CGSize(View.Frame.Width, View.Frame.Height - NavigationController.View.Frame.Height);
            if (TabBarController != null)
                TabBarController.TabBar.Hidden = true;
            NavigationController.SetNavigationBarHidden(false, false);
        }
    }
}

