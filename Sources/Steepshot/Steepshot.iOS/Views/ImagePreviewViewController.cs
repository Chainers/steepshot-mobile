using System;
using CoreGraphics;
using FFImageLoading;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class ImagePreviewViewController : UIViewController
    {
        private UIImage _imageForPreview;
        private string _imageUrl;

        protected ImagePreviewViewController(IntPtr handle) : base(handle) { }

        public ImagePreviewViewController(string imageUrl, UIImage ImageForPreview = null)
        {
            _imageUrl = imageUrl;
            _imageForPreview = ImageForPreview;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var margin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            var imageScrollView = new UIScrollView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - margin));
            View.AddSubview(imageScrollView);
            var imageView = new UIImageView(new CGRect(0, 0, imageScrollView.Frame.Width, imageScrollView.Frame.Height));
            imageScrollView.MinimumZoomScale = 1f;
            imageScrollView.MaximumZoomScale = 6.0f;
            imageScrollView.ViewForZoomingInScrollView += (UIScrollView sv) => { return imageView; };
            View.BackgroundColor = UIColor.White;

            if (_imageForPreview != null)
                imageView.Image = _imageForPreview;
            else
                imageView.Image = UIImage.FromBundle("ic_photo_holder");

            ImageService.Instance.LoadUrl(_imageUrl, Constants.ImageCacheDuration)
                                                     .Retry(5)
                                                     .Into(imageView);

            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            imageScrollView.ContentSize = imageView.Image.Size;
            imageScrollView.AddSubview(imageView);
            imageScrollView.ContentSize = new CGSize(imageScrollView.Frame.Width, imageScrollView.Frame.Height);
            SetBackButton();
        }

        private void SetBackButton()
        {
            NavigationItem.Title = "Photo preview";
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            leftBarButton.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.LeftBarButtonItem = leftBarButton;
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }
    }
}
