using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Helpers;
using Steepshot.Core.Extensions;
using UIKit;
using Steepshot.Core.Models.Requests;

namespace Steepshot.iOS.Cells
{
    public class CardCollectionViewCell : UICollectionViewCell
    {
        private UIImageView _image = new UIImageView();
        private UIView _shadowHelper = new UIView();

        private UILabel _login = new UILabel();
        private UILabel _equivalentBalance = new UILabel();

        private UILabel _firstTokenLabel = new UILabel();
        private UILabel _firstTokenValue = new UILabel();

        private UILabel _secondTokenLabel = new UILabel();
        private UILabel _secondTokenValue = new UILabel();

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

            _login.Font = Constants.Semibold14;
            _login.TextColor = Constants.R255G255B255;
            loginBackground.AddSubview(_login);

            _login.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _login.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            _login.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 21);

            var balanceLabel = new UILabel();
            balanceLabel.Text = "Account balance";
            balanceLabel.Font = Constants.Semibold14;
            balanceLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            _image.AddSubview(balanceLabel);

            balanceLabel.AutoAlignAxis(ALAxis.Horizontal, loginBackground);
            balanceLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);

            _equivalentBalance.Font = Constants.Bold34;
            _equivalentBalance.TextColor = Constants.R255G255B255;
            _image.AddSubview(_equivalentBalance);

            _equivalentBalance.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            _equivalentBalance.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, loginBackground);
            _equivalentBalance.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var bottomContainer = new UIView();
            _image.AddSubview(bottomContainer);

            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            bottomContainer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _equivalentBalance);

            var steemContainer = new UIView();
            bottomContainer.AddSubview(steemContainer);

            steemContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            _firstTokenLabel.Text = "Steem";
            _firstTokenLabel.Font = Constants.Semibold14;
            _firstTokenLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemContainer.AddSubview(_firstTokenLabel);

            _firstTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _firstTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _firstTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            _firstTokenValue.Text = "999.999";
            _firstTokenValue.Font = Constants.Semibold20;
            _firstTokenValue.TextColor = Constants.R255G255B255;
            steemContainer.AddSubview(_firstTokenValue);

            _firstTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _firstTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _firstTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _firstTokenValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _firstTokenLabel);

            var steemPowerContainer = new UIView();
            bottomContainer.AddSubview(steemPowerContainer);

            steemPowerContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemPowerContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            _secondTokenLabel.Text = "Steem Power";
            _secondTokenLabel.Font = Constants.Semibold14;
            _secondTokenLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemPowerContainer.AddSubview(_secondTokenLabel);

            _secondTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _secondTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _secondTokenLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            _secondTokenValue.Text = "999.999";
            _secondTokenValue.Font = Constants.Semibold20;
            _secondTokenValue.TextColor = Constants.R255G255B255;
            steemPowerContainer.AddSubview(_secondTokenValue);

            _secondTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _secondTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _secondTokenValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _secondTokenValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _secondTokenLabel);
        }

        public void UpdateCard(BalanceModel balance, CurrencyRate currencyRate, int i)
        {
            _login.Text = $"@{balance.UserInfo.Login}";
            double usdBalance = 0;

            switch (balance.CurrencyType)
            {
                case CurrencyType.Steem:
                case CurrencyType.Golos:
                    {
                        _secondTokenLabel.Text = $"{balance.CurrencyType.ToString()} Power".ToUpper();
                        usdBalance = (balance.Value + balance.EffectiveSp) * (currencyRate?.UsdRate ?? 1);
                        break;
                    }
                case CurrencyType.Sbd:
                case CurrencyType.Gbg:
                    {
                        _secondTokenLabel.Hidden = true;
                        _secondTokenValue.Hidden = true;
                        usdBalance = balance.Value * (currencyRate?.UsdRate ?? 1);
                        break;
                    }
            }

            _equivalentBalance.Text = $"$ {usdBalance.ToBalanceVaueString()}".ToUpper();
            _firstTokenLabel.Text = balance.CurrencyType.ToString().ToUpper();
            _firstTokenValue.Text = balance.Value.ToBalanceVaueString();
            _secondTokenValue.Text = balance.EffectiveSp.ToBalanceVaueString();

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
