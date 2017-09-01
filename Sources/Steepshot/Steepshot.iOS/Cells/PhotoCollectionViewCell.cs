using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Photos;
using Steepshot.Core.Models.Responses;
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

		static PhotoCollectionViewCell()
		{
			Nib = UINib.FromName(nameof(PhotoCollectionViewCell), NSBundle.MainBundle);
		}

		public void UpdateImage(UIImage photo, PHAsset asset)
		{
			photoImg.Image = photo;
			Asset = asset;
		}

		public override void UpdateCell(Post post, NSMutableAttributedString comment)
		{
			ImageUrl = post.Body;
			photoImg.Image = null;
			_scheduledWork?.Cancel();
			_scheduledWork = ImageService.Instance.LoadUrl(post.Body, Constants.ImageCacheDuration)
										 .Retry(2, 200)
										 .DownSample(width: _downSampleWidth)
										 .Into(photoImg);
		}
		/*
		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(UICollectionViewLayoutAttributes layoutAttributes)
		{
			layoutAttributes.Frame = new CGRect(layoutAttributes.Frame.Location, Constants.CellSize);
			return layoutAttributes;
		}*/
	}
}
