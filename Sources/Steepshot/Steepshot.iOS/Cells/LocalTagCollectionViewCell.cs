using System;
using Foundation;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class LocalTagCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("LocalTagCollectionViewCell");
        public static readonly UINib Nib;

        private bool _isInitialized;

        static LocalTagCollectionViewCell()
        {
            Nib = UINib.FromName("LocalTagCollectionViewCell", NSBundle.MainBundle);
        }

        protected LocalTagCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void LayoutSubviews()
        {
            if (!_isInitialized)
            {
                hashTag.Font = tagText.Font = Constants.Semibold14;

                rootView.Layer.CornerRadius = 20;
                rootView.Layer.BorderColor = Constants.R244G244B246.CGColor;
                rootView.Layer.BorderWidth = 1;
                _isInitialized = true;
            }
            base.LayoutSubviews();
        }

        public void RefreshCell(string cellText)
        {
            tagText.Text = cellText;
            LayoutIfNeeded();
        }
    }
}
