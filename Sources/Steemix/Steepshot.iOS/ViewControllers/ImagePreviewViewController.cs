using System;
using CoreGraphics;
using UIKit;

namespace Steepshot.iOS
{
	public partial class ImagePreviewViewController : UIViewController
	{
		public UIImage imageForPreview;

		protected ImagePreviewViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var imageScrollView = new UIScrollView(new CGRect(0, NavigationController.NavigationBar.Frame.Height, View.Frame.Width, View.Frame.Height - NavigationController.NavigationBar.Frame.Height));
			this.View.AddSubview(imageScrollView);
			var imageView = new UIImageView(new CGRect(0, 0, imageScrollView.Frame.Width, imageScrollView.Frame.Height));
			if (imageForPreview != null)
			{
				imageView.Image = imageForPreview;
				imageScrollView.MinimumZoomScale = 1f;
				imageScrollView.MaximumZoomScale = 6.0f;
				imageScrollView.ViewForZoomingInScrollView += (UIScrollView sv) => { return imageView; };
			}
			else
			{
				backgroundView.BackgroundColor = UIColor.White;
				imageView.Image = UIImage.FromBundle("ic_photo_holder");
			}
			imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			imageScrollView.ContentSize = imageView.Image.Size;
			imageScrollView.AddSubview(imageView);
			imageScrollView.ContentSize = new CGSize(this.View.Frame.Width, this.View.Frame.Height - NavigationController.View.Frame.Height);

		}
	}
}

