using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class TransactionShimmerCollectionViewCell : UICollectionViewCell
    {
        private UIView _action = new UIView();
        private UIView _amount = new UIView();
        private UIView _topLine;
        private UIView _bottomLine;

        protected TransactionShimmerCollectionViewCell(IntPtr handle) : base(handle)
        {
            var sideMargin = DeviceHelper.IsSmallDevice ? 15 : 30;

            var background = new CustomView();
            background.BackgroundColor = Constants.R230G230B230;
            background.Layer.CornerRadius = 16;
            ContentView.AddSubview(background);

            background.AutoPinEdgesToSuperviewEdges(new UIEdgeInsets(5, 60, 5, 10));

            var emptyBackground = new UIView();
            ContentView.AddSubview(emptyBackground);

            emptyBackground.AutoPinEdge(ALEdge.Right, ALEdge.Left, background);
            emptyBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            emptyBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            emptyBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var leftContainer = new CustomView();
            background.AddSubview(leftContainer);

            leftContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, sideMargin);
            leftContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            _action.ClipsToBounds = true;
            _action.Layer.CornerRadius = 7.5f;
            _action.BackgroundColor = UIColor.White.ColorWithAlpha(0.5f);
            leftContainer.AddSubview(_action);

            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _action.AutoSetDimensionsToSize(new CGSize(80, 15));

            var to = new UIView();
            to.ClipsToBounds = true;
            to.Layer.CornerRadius = 7.5f;
            to.BackgroundColor = UIColor.White.ColorWithAlpha(0.5f);
            leftContainer.AddSubview(to);

            to.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _action, 10);
            to.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            to.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            to.AutoSetDimensionsToSize(new CGSize(120, 15));

             _amount.ClipsToBounds = true;
            _amount.Layer.CornerRadius = 10f;
            _amount.BackgroundColor = UIColor.White.ColorWithAlpha(0.5f);
            background.AddSubview(_amount);

            _amount.AutoSetDimensionsToSize(new CGSize(80, 20));
            _amount.AutoPinEdge(ALEdge.Left, ALEdge.Right, leftContainer);
            _amount.AutoAlignAxis(ALAxis.Horizontal, leftContainer);
            _amount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, sideMargin);

            var circle = new UIView();
            circle.BackgroundColor = Constants.R230G230B230;
            circle.Layer.CornerRadius = 4;
            emptyBackground.AddSubview(circle);

            circle.AutoSetDimensionsToSize(new CGSize(8, 8));
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);

            _topLine = new UIView();
            _topLine.BackgroundColor = Constants.R240G240B240;
            _topLine.Layer.CornerRadius = 1;
            emptyBackground.AddSubview(_topLine);

            _topLine.AutoSetDimension(ALDimension.Width, 2);
            _topLine.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _topLine.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _topLine.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, circle, -16);

            _bottomLine = new UIView();
            _bottomLine.BackgroundColor = Constants.R240G240B240;
            _bottomLine.Layer.CornerRadius = 1;
            emptyBackground.AddSubview(_bottomLine);

            _bottomLine.AutoSetDimension(ALDimension.Width, 2);
            _bottomLine.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _bottomLine.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _bottomLine.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, circle, 16);

            leftContainer.SubviewLayouted += () =>
            {
                Constants.ApplyShimmer(_action);
                Constants.ApplyShimmer(to);
            };

            background.SubviewLayouted += () =>
            {
                Constants.ApplyShimmer(_amount);
            };
        }

        public void UpdateCard(bool isFirst, bool isLast)
        {
            _topLine.Hidden = isFirst;
            _bottomLine.Hidden = isLast;
        }
    }
}
