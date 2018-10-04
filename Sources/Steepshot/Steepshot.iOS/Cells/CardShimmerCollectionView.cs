using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class CardShimmerCollectionView : UICollectionViewCell
    {
        private readonly UIView _shadowHelper = new UIView();
        private readonly UILabel _login = new UILabel();
        private readonly UILabel _equivalentBalance = new UILabel();
        private readonly UILabel _firstTokenLabel = new UILabel();
        private readonly UILabel _firstTokenValue = new UILabel();
        private readonly UILabel _secondTokenLabel = new UILabel();
        private readonly UILabel _secondTokenValue = new UILabel();

        protected CardShimmerCollectionView(IntPtr handle) : base(handle)
        {
            CustomView _image = new CustomView();
            _image.BackgroundColor = UIColor.FromRGB(230, 230, 230);
            _image.ClipsToBounds = true;
            _image.Layer.CornerRadius = 15;
            ContentView.AddSubview(_image);
            _image.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _image.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _image.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _image.AutoSetDimension(ALDimension.Height, ContentView.Frame.Width * 190f / 335f);

            _shadowHelper.BackgroundColor = UIColor.White;
            ContentView.AddSubview(_shadowHelper);
            _shadowHelper.AutoPinEdge(ALEdge.Left, ALEdge.Left, _image, 20);
            _shadowHelper.AutoPinEdge(ALEdge.Right, ALEdge.Right, _image, -20);
            _shadowHelper.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, _image);
            _shadowHelper.AutoPinEdge(ALEdge.Top, ALEdge.Top, _image);
            ContentView.BringSubviewToFront(_image);

            var loginBackground = new UIView();
            loginBackground.ClipsToBounds = true;
            loginBackground.Layer.CornerRadius = 15;
            loginBackground.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.5f);

            _image.AddSubview(loginBackground);

            loginBackground.AutoSetDimensionsToSize(new CGSize(140, 32));
            loginBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right, -20);
            loginBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 21);

            UILabel _balanceLabel = new UILabel();
            _balanceLabel.Font = Constants.Semibold14;
            _balanceLabel.Text = "Steem balance";
            _balanceLabel.ClipsToBounds = true;
            _balanceLabel.Layer.CornerRadius = 9;
            _balanceLabel.TextColor = UIColor.Clear;
            _balanceLabel.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            _image.AddSubview(_balanceLabel);

            _balanceLabel.AutoAlignAxis(ALAxis.Horizontal, loginBackground);
            _balanceLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);

            _equivalentBalance.Text = "2422,222";
            _equivalentBalance.Font = Constants.Bold34;
            _equivalentBalance.ClipsToBounds = true;
            _equivalentBalance.Layer.CornerRadius = 22;
            _equivalentBalance.TextColor = UIColor.Clear;
            _equivalentBalance.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            _image.AddSubview(_equivalentBalance);

            _equivalentBalance.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            _equivalentBalance.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, loginBackground);

            var bottomContainer = new UIView();
            _image.AddSubview(bottomContainer);

            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            bottomContainer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _equivalentBalance);

            var steemContainer = new CustomView();
            bottomContainer.AddSubview(steemContainer);

            steemContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            _firstTokenLabel.Text = "Steem";
            _firstTokenLabel.ClipsToBounds = true;
            _firstTokenLabel.Layer.CornerRadius = 10;
            _firstTokenLabel.TextColor = UIColor.Clear;
            _firstTokenLabel.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            _firstTokenLabel.Font = Constants.Semibold14;
            steemContainer.AddSubview(_firstTokenLabel);

            _firstTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _firstTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            _firstTokenValue.Text = "999.999";
            _firstTokenValue.ClipsToBounds = true;
            _firstTokenValue.Font = Constants.Semibold20;
            _firstTokenValue.Layer.CornerRadius = 15;
            _firstTokenValue.TextColor = UIColor.Clear;
            _firstTokenValue.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemContainer.AddSubview(_firstTokenValue);

            _firstTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _firstTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _firstTokenValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _firstTokenLabel, 5);

            var steemPowerContainer = new CustomView();
            bottomContainer.AddSubview(steemPowerContainer);

            steemPowerContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemPowerContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            _secondTokenLabel.ClipsToBounds = true;
            _secondTokenLabel.Text = "Steem Power";
            _secondTokenLabel.Font = Constants.Semibold14;
            _secondTokenLabel.Layer.CornerRadius = 10;
            _secondTokenLabel.TextColor = UIColor.Clear;
            _secondTokenLabel.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            _secondTokenLabel.Font = Constants.Semibold14;
            steemPowerContainer.AddSubview(_secondTokenLabel);

            _secondTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _secondTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            _secondTokenValue.ClipsToBounds = true;
            _secondTokenValue.Text = "999.999";
            _secondTokenValue.Font = Constants.Semibold20;
            _secondTokenValue.Layer.CornerRadius = 15;
            _secondTokenValue.TextColor = UIColor.Clear;
            _secondTokenValue.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemPowerContainer.AddSubview(_secondTokenValue);

            _secondTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _secondTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _secondTokenValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _secondTokenLabel, 5);

            _image.SubviewLayouted += () =>
            {
                Constants.ApplyShimmer(_balanceLabel);
                Constants.ApplyShimmer(loginBackground);
                Constants.ApplyShimmer(_equivalentBalance);
            };

            steemContainer.SubviewLayouted += () =>
            {
                Constants.ApplyShimmer(_firstTokenLabel);
                Constants.ApplyShimmer(_firstTokenValue);
            };

            steemContainer.SubviewLayouted += () =>
            {
                Constants.ApplyShimmer(_secondTokenLabel);
                Constants.ApplyShimmer(_secondTokenValue);
            };
        }
    }
}
