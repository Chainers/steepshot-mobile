using System;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class LoaderCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(LoaderCell));
        private UIActivityIndicatorView loader;

        protected LoaderCell(NSObjectFlag t) : base(t) { }

        protected internal LoaderCell(IntPtr handle) : base(handle) { }

        public override void LayoutSubviews()
        {
            if (loader == null)
            {
                loader = new UIActivityIndicatorView();
                loader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
                loader.Color = Helpers.Constants.R231G72B0;
                this.AddSubview(loader);
                loader.AutoCenterInSuperview();
                loader.AutoSetDimensionsToSize(new CGSize(35, 35));
            }
            loader.StartAnimating();
        }
    }
}
