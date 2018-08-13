using System;
using System.Globalization;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.Core.Extensions;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class PowerManipulationViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        private readonly BalanceModel _balance;
        private readonly PowerAction _powerAction;
        private double _powerAmount;
        private SearchTextField _amount;
        private UILabel _firstTokenText = new UILabel();
        private UILabel _secondTokenText = new UILabel();
        private UIActivityIndicatorView _loader = new UIActivityIndicatorView();
        private UIButton _actionButton = new UIButton();

        public PowerManipulationViewController(BalanceModel balance, PowerAction action)
        {
            _balance = balance;
            _powerAction = action;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetBackButton();
            CreateView();
            AmountEditOnTextChanged(null, null);
        }

        private void AmountEditOnTextChanged(object sender, EventArgs e)
        {
            //_amountLimitMessage.Visibility = ViewStates.Gone;

            if (string.IsNullOrEmpty(_amount.Text))
            {
                UpdateTokenValues(_balance.Value.ToBalanceValueString(), _balance.Value.ToBalanceValueString(), _balance.EffectiveSp.ToBalanceValueString(), _balance.EffectiveSp.ToBalanceValueString());
                _powerAmount = -1;
                return;
            }

            var amountEdit = double.Parse(_amount.Text, CultureInfo.InvariantCulture);
            var amountAvailable = _balance.Value;
            var spAvailiable = _balance.EffectiveSp;

            if (amountEdit <= (_powerAction == PowerAction.PowerUp ? amountAvailable : spAvailiable))
            {
                switch (_powerAction)
                {
                    case PowerAction.PowerUp:
                        UpdateTokenValues(_balance.Value.ToBalanceValueString(), (amountAvailable - amountEdit).ToBalanceValueString(), _balance.EffectiveSp.ToBalanceValueString(), (spAvailiable + amountEdit).ToBalanceValueString());
                        break;
                    case PowerAction.PowerDown:
                        UpdateTokenValues(_balance.Value.ToBalanceValueString(), (amountAvailable + amountEdit).ToBalanceValueString(), _balance.EffectiveSp.ToBalanceValueString(), (spAvailiable - amountEdit).ToBalanceValueString());
                        break;
                }
                _powerAmount = amountEdit;
            }
            else
            {
                UpdateTokenValues(_balance.Value.ToBalanceValueString(), AppSettings.LocalizationManager.GetText(LocalizationKeys.AmountLimit), _balance.EffectiveSp.ToBalanceValueString(), AppSettings.LocalizationManager.GetText(LocalizationKeys.AmountLimit));
                _powerAmount = -1;
                //_amountLimitMessage.Visibility = ViewStates.Visible;
            }
        }

        private UIStringAttributes _noLinkAttribute = new UIStringAttributes
        {
            Font = Constants.Regular24,
            ForegroundColor = Constants.R151G155B158,
        };
        private UIStringAttributes linkAttribute = new UIStringAttributes
        {
            Font = Constants.Regular24,
            ForegroundColor = Constants.R255G34B5,
        };

        private void UpdateTokenValues(string currTokenOne, string nextTokenOne, string currTokenTwo, string nextTokenTwo)
        {
            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString($"{currTokenOne} >", _noLinkAttribute));
            at.Append(new NSAttributedString($" {nextTokenOne}", linkAttribute));
            _firstTokenText.AttributedText = at;

            var at2 = new NSMutableAttributedString();
            at2.Append(new NSAttributedString($"{currTokenTwo} >", _noLinkAttribute));
            at2.Append(new NSAttributedString($" {nextTokenTwo}", linkAttribute));
            _secondTokenText.AttributedText = at2;
        }

        private void MaxBtnOnClick(object sender, EventArgs e)
        {
            _amount.Text = _powerAction == PowerAction.PowerUp ? _balance.Value.ToBalanceValueString() : (_balance.EffectiveSp - _balance.DelegatedToMe).ToBalanceValueString();
            AmountEditOnTextChanged(null, null);
        }

        private void PowerBtnOnClick(object sender, EventArgs e)
        {
            if (_powerAmount <= 0)
                return;

            if (string.IsNullOrEmpty(_balance.UserInfo.ActiveKey))
            {
                NavigationController.PushViewController(new LoginViewController(false), true);
                return;
            }

            DoPowerAction();
        }

        private async void DoPowerAction()
        {
            _loader.StartAnimating();
            _actionButton.Enabled = false;

            var model = new BalanceModel(_powerAmount, _balance.MaxDecimals, _balance.CurrencyType)
            {
                UserInfo = _balance.UserInfo
            };

            var response = await _presenter.TryPowerUpOrDown(model, _powerAction);

            _loader.StopAnimating();
            _actionButton.Enabled = true;

            if (response.IsSuccess)
            {
                NavigationController.PopViewController(true);
            }
            else
            {
                ShowAlert(response.Exception);
            }
        }

        private void CreateView()
        {
            var minAmount = 0.001;

            View.UserInteractionEnabled = true;

            var viewTap = new UITapGestureRecognizer(() =>
            {
                _amount.ResignFirstResponder();
            });

            View.AddGestureRecognizer(viewTap);

            View.BackgroundColor = Constants.R250G250B250;

            var topBackground = new UIView();
            topBackground.BackgroundColor = UIColor.White;
            View.AddSubview(topBackground);

            topBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            topBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            topBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var steemView = new UIView();
            topBackground.AddSubview(steemView);

            var label = new UILabel();
            label.Text = "Steem";
            label.Font = Constants.Semibold14;
            steemView.AddSubview(label);

            label.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            _firstTokenText = new UILabel();
            _firstTokenText.Text = "4 > 5";
            steemView.AddSubview(_firstTokenText);

            _firstTokenText.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _firstTokenText.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _firstTokenText.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            steemView.AutoSetDimension(ALDimension.Height, 70);
            steemView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            steemView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            steemView.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;

            topBackground.AddSubview(separator);

            separator.AutoSetDimension(ALDimension.Height, 1);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemView);

            var spView = new UIView();
            topBackground.AddSubview(spView);

            var label2 = new UILabel();
            label2.Text = "SteemPower";
            label2.Font = Constants.Semibold14;
            spView.AddSubview(label2);

            label2.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            label2.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            _secondTokenText.Text = "4 > 8";
            spView.AddSubview(_secondTokenText);

            _secondTokenText.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _secondTokenText.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _secondTokenText.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            spView.AutoSetDimension(ALDimension.Height, 70);
            spView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator);
            spView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            spView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            spView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var amountBackground = new UIView();
            amountBackground.BackgroundColor = UIColor.White;
            View.AddSubview(amountBackground);

            amountBackground.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, topBackground, 10);
            amountBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            amountBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var amountLabel = new UILabel();
            amountLabel.Text = "Amount";
            amountLabel.Font = Constants.Semibold14;
            amountBackground.AddSubview(amountLabel);

            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15);
            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);

            _amount = new SearchTextField(() =>
            {

            }, AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint), new AmountFieldDelegate());
            _amount.Text = minAmount.ToString(CultureInfo.InvariantCulture);
            _amount.EditingChanged += AmountEditOnTextChanged;
            _amount.KeyboardType = UIKeyboardType.DecimalPad;
            _amount.Layer.CornerRadius = 25;
            amountBackground.AddSubview(_amount);

            _amount.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            _amount.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountLabel, 16);
            _amount.AutoSetDimension(ALDimension.Height, 50);
            _amount.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);

            _amount.TouchUpOutside += (object sender, EventArgs e) =>
            {
                _amount.ResignFirstResponder();
            };

            var max = new UIButton();
            max.SetTitle("MAX", UIControlState.Normal);
            max.SetTitleColor(UIColor.Black, UIControlState.Normal);
            max.Font = Constants.Semibold14;
            max.Layer.BorderWidth = 1;
            max.Layer.BorderColor = Constants.R245G245B245.CGColor;
            max.Layer.CornerRadius = 25;

            amountBackground.AddSubview(max);

            max.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            max.AutoPinEdge(ALEdge.Left, ALEdge.Right, _amount, 10);
            max.AutoSetDimensionsToSize(new CGSize(80, 50));
            max.AutoAlignAxis(ALAxis.Horizontal, _amount);
            max.TouchDown += MaxBtnOnClick;

            _actionButton.SetTitle(AppSettings.LocalizationManager.GetText(_powerAction == PowerAction.PowerUp ? LocalizationKeys.PowerUp : LocalizationKeys.PowerDown), UIControlState.Normal);
            _actionButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _actionButton.SetTitleColor(UIColor.Clear, UIControlState.Disabled);

            View.AddSubview(_actionButton);

            _actionButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountBackground, 30);
            _actionButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            _actionButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            _actionButton.AutoSetDimension(ALDimension.Height, 50);

            _actionButton.LayoutIfNeeded();
            _actionButton.TouchDown += PowerBtnOnClick;

            _loader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            _loader.HidesWhenStopped = true;
            _actionButton.AddSubview(_loader);
            _loader.AutoCenterInSuperview();

            Constants.CreateGradient(_actionButton, 25);
            Constants.CreateShadowFromZeplin(_actionButton, Constants.R231G72B0, 0.3f, 0, 10, 20, 0);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppSettings.LocalizationManager.GetText(_powerAction == PowerAction.PowerUp ? LocalizationKeys.PowerUp : LocalizationKeys.PowerDown);

            NavigationController.NavigationBar.Translucent = false;
        }
    }
}
