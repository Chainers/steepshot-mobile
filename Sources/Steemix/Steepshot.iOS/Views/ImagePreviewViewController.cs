﻿using System;
using CoreGraphics;
using FFImageLoading;
using UIKit;

namespace Steepshot.iOS
{
	public partial class ImagePreviewViewController : UIViewController
	{
		protected ImagePreviewViewController(IntPtr handle) : base(handle) { }

		public ImagePreviewViewController()
		{
		}

		public UIImage imageForPreview;
		public string ImageUrl;

		public override void ViewWillAppear(bool animated)
		{
			NavigationController.SetNavigationBarHidden(false, true);
			base.ViewWillAppear(animated);
		}

		public override void ViewWillDisappear(bool animated)
		{
			NavigationController.SetNavigationBarHidden(true, true);
			base.ViewWillDisappear(animated);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			var margin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
			var imageScrollView = new UIScrollView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - margin));
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
				this.View.BackgroundColor = UIColor.White;
				imageView.Image = UIImage.FromBundle("ic_photo_holder");
			}
			ImageService.Instance.LoadUrl(ImageUrl, Constants.ImageCacheDuration)
													 .Retry(2, 200)
													 .Into(imageView);
			imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			imageScrollView.ContentSize = imageView.Image.Size;
			imageScrollView.AddSubview(imageView);
			imageScrollView.ContentSize = new CGSize(imageScrollView.Frame.Width, imageScrollView.Frame.Height);
			if (TabBarController != null)
				TabBarController.TabBar.Hidden = true;
		}
	}
}

