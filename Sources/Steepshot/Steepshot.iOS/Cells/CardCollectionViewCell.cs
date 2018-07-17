using System;
using CoreAnimation;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class CardCollectionViewCell : UICollectionViewCell
    {
        public UIView root;
        private UILabel _label;
        private UIImageView _image = new UIImageView();
        private UIView _shadowHelper = new UIView();

        public override void LayoutSubviews()
        {
            var absolutePosition = _image.ConvertRectToView(_image.Frame, root);
        }

        protected CardCollectionViewCell(IntPtr handle) : base(handle)
        {
            _image.Image = UIImage.FromBundle("ic_blue_card");
            ContentView.AddSubview(_image);
            _image.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _image.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _image.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _image.SizeToFit();

            _shadowHelper.BackgroundColor = UIColor.White;
            ContentView.AddSubview(_shadowHelper);
            _shadowHelper.AutoPinEdge(ALEdge.Left, ALEdge.Left, _image, 20);
            _shadowHelper.AutoPinEdge(ALEdge.Right, ALEdge.Right, _image, -20);
            _shadowHelper.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, _image);
            _shadowHelper.AutoPinEdge(ALEdge.Top, ALEdge.Top, _image);

            LAL(_shadowHelper, Constants.R74G144B226, 0.3f, 0, 20, 30, 0);

            ContentView.BringSubviewToFront(_image);
           
           

            //115

            //ContentView.BackgroundColor = UIColor.Cyan;
            /*
            _image.Layer.CornerRadius = 0;
            _image.Layer.MasksToBounds = false;
            _image.Layer.ShadowOffset = new CGSize(0f, 20);
            _image.Layer.ShadowRadius = 4;
            _image.Layer.ShadowOpacity = 1;
            _image.Layer.ShadowColor = Constants.R74G144B226.ColorWithAlpha(0.3f).CGColor;
*/



            //Constants.CreateShadow(_image, Constants.R74G144B226, 0.5f, 25, 10, 12);

            //_image.BackgroundColor = UIColor.DarkGray;

            //_label = new UILabel();
            //_label.Text = "TRALALALLALALA";

            //_label.BackgroundColor = UIColor.Cyan;
            //ContentView.BackgroundColor = UIColor.Cyan;
            //ContentView.AddSubview(_label);

            //_label.AutoCenterInSuperview();
        }

        private void LAL(UIView view, UIColor color, float alpha, float x, float y, float blur, float spread)
        {
            {
                view.Layer.MasksToBounds = false;
                view.Layer.ShadowColor = color.CGColor;
                view.Layer.ShadowOpacity = alpha;
                view.Layer.ShadowOffset = new CGSize(x, y);
                view.Layer.ShadowRadius = blur / 2f;
                if (spread == 0)
                {
                    view.Layer.ShadowPath = null;
                }
                else
                {
                    var dx = -spread;
                    var rect = view.Layer.Bounds.Inset(dx, dx);
                    view.Layer.ShadowPath = UIBezierPath.FromRect(rect).CGPath;
                }
            }
        }
    }
}
