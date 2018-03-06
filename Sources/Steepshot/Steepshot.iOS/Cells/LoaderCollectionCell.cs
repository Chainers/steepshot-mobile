using System;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class LoaderCollectionCell : UICollectionViewCell
    {
        private UIActivityIndicatorView loader;

        public static readonly NSString Key = new NSString("LoaderCollectionCell");

        static LoaderCollectionCell()
        {
        }

        protected LoaderCollectionCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public void SetLoader()
        {
            //Move to LayoutSubviews?
            if (loader == null)
            {
                loader = new UIActivityIndicatorView();
                loader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
                loader.Color = Helpers.Constants.R231G72B0;
                ContentView.AddSubview(loader);
                loader.AutoCenterInSuperview();
                loader.AutoSetDimensionsToSize(new CGSize(35, 35));
            }
            loader.StartAnimating();
        }
    }
}
