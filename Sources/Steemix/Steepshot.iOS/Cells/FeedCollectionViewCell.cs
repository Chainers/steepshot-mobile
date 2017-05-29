using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Steepshot.iOS
{
	public partial class FeedCollectionViewCell : UICollectionViewCell
	{
		public static readonly NSString Key = new NSString("FeedCollectionViewCell");
		public static readonly UINib Nib;

		static FeedCollectionViewCell()
		{
			Nib = UINib.FromName("FeedCollectionViewCell", NSBundle.MainBundle);
		}

		protected FeedCollectionViewCell(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(UICollectionViewLayoutAttributes layoutAttributes)
		{
			SetNeedsLayout();
			LayoutIfNeeded();
			var size = contentView.SystemLayoutSizeFittingSize(layoutAttributes.Size);
 			var newFrame = layoutAttributes.Frame;
			newFrame.Size = new CGSize(newFrame.Size.Width, size.Height);
			layoutAttributes.Frame = newFrame;
			return layoutAttributes;
		}
	}
}
