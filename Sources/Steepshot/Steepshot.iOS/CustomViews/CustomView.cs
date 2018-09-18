using System;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class CustomView : UIView
    {
        public Action SubviewLayouted;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            SubviewLayouted?.Invoke();
        }
    }
}
