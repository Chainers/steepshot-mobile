using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class CardCollectionViewCell : UICollectionViewCell
    {
        private UIImageView _image = new UIImageView();
        private UIView _shadowHelper = new UIView();

        protected CardCollectionViewCell(IntPtr handle) : base(handle)
        {
            _image.ClipsToBounds = true;
            _image.ContentMode = UIViewContentMode.ScaleAspectFill;
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
            loginBackground.Layer.CornerRadius = 15;
            loginBackground.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.1f);

            _image.AddSubview(loginBackground);

            loginBackground.AutoSetDimensionsToSize(new CGSize(140, 32));
            loginBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right, -20);
            loginBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 21);

            var login = new UILabel();
            login.Text = "@steepshot";
            login.Font = Constants.Semibold14;
            login.TextColor = Constants.R255G255B255;
            loginBackground.AddSubview(login);

            login.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            login.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            login.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 21);

            var balanceLabel = new UILabel();
            balanceLabel.Text = "Account balance";
            balanceLabel.Font = Constants.Semibold14;
            balanceLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            _image.AddSubview(balanceLabel);

            balanceLabel.AutoAlignAxis(ALAxis.Horizontal, loginBackground);
            balanceLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);

            var equivalentBalance = new UILabel();
            equivalentBalance.Text = "$ 1 999.999";
            equivalentBalance.Font = Constants.Bold34;
            equivalentBalance.TextColor = Constants.R255G255B255;
            _image.AddSubview(equivalentBalance);

            equivalentBalance.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            equivalentBalance.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, loginBackground);
            equivalentBalance.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var bottomContainer = new UIView();
            _image.AddSubview(bottomContainer);

            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            bottomContainer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, equivalentBalance);

            var steemContainer = new UIView();
            bottomContainer.AddSubview(steemContainer);

            steemContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            var steemLabel = new UILabel();
            steemLabel.Text = "Steem";
            steemLabel.Font = Constants.Semibold14;
            steemLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemContainer.AddSubview(steemLabel);

            steemLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            var steemValue = new UILabel();
            steemValue.Text = "999.999";
            steemValue.Font = Constants.Semibold20;
            steemValue.TextColor = Constants.R255G255B255;
            steemContainer.AddSubview(steemValue);

            steemValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            steemValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemLabel);

            var steemPowerContainer = new UIView();
            bottomContainer.AddSubview(steemPowerContainer);

            steemPowerContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemPowerContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            var steemPowerLabel = new UILabel();
            steemPowerLabel.Text = "Steem Power";
            steemPowerLabel.Font = Constants.Semibold14;
            steemPowerLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemPowerContainer.AddSubview(steemPowerLabel);

            steemPowerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemPowerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            var steemPowerValue = new UILabel();
            steemPowerValue.Text = "999.999";
            steemPowerValue.Font = Constants.Semibold20;
            steemPowerValue.TextColor = Constants.R255G255B255;
            steemPowerContainer.AddSubview(steemPowerValue);

            steemPowerValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemPowerValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            steemPowerValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemPowerLabel);
        }

        public void UpdateCard(int i)
        {
            switch (i)
            {
                case 0:
                    {
                        _image.Image = UIImage.FromBundle("ic_red_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, UIColor.FromRGB(232, 56, 31), 0.3f, 0, 20, 30, 0);
                        break;
                    }
                case 1:
                    {
                        _image.Image = UIImage.FromBundle("ic_blue_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, Constants.R74G144B226, 0.3f, 0, 20, 30, 0);
                        break;
                    }
                case 2:
                    {
                        _image.Image = UIImage.FromBundle("ic_green_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, UIColor.FromRGB(103, 184, 47), 0.3f, 0, 20, 30, 0);
                        break;
                    }
                case 3:
                    {
                        _image.Image = UIImage.FromBundle("ic_orange_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, UIColor.FromRGB(218, 146, 44), 0.3f, 0, 20, 30, 0);
                        break;
                    }
                case 4:
                    {
                        _image.Image = UIImage.FromBundle("ic_pink_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, UIColor.FromRGB(189, 16, 224), 0.3f, 0, 20, 30, 0);
                        break;
                    }
                case 5:
                    {
                        _image.Image = UIImage.FromBundle("ic_purple_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, UIColor.FromRGB(144, 19, 254), 0.3f, 0, 20, 30, 0);
                        break;
                    }
                case 6:
                    {
                        _image.Image = UIImage.FromBundle("ic_grey_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, UIColor.FromRGB(155, 155, 155), 0.3f, 0, 20, 30, 0);
                        break;
                    }
                case 7:
                    {
                        _image.Image = UIImage.FromBundle("ic_light_red_card");
                        Constants.CreateShadowFromZeplin(_shadowHelper, UIColor.FromRGB(208, 2, 166), 0.3f, 0, 20, 30, 0);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
