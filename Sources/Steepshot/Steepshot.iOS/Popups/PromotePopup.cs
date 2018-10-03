using System;
using System.Collections.Generic;
using System.Threading;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Popups
{
    public class PromotePopup
    {
        private List<CurrencyType> _coins;
        private CurrencyType _pickedCoin = CurrencyType.Steem;
        private SearchTextField _amountTextField;
        private UIActivityIndicatorView balanceLoader;
        private UILabel balanceLabel;
        private UILabel errorMessage;
        private List<BalanceModel> balances;
        private readonly PromotePresenter _presenter;

        public PromotePopup()
        {
            _presenter = AppSettings.GetPresenter<PromotePresenter>(AppSettings.MainChain);
        }

        private async void GetBalance()
        {
            balanceLoader.StartAnimating();
            var response = await _presenter.TryGetAccountInfoAsync(AppSettings.User.Login);

            if (response.IsSuccess)
            {
                balances = response.Result?.Balances;
                var balance = balances?.Find(x => x.CurrencyType == _pickedCoin);
                balanceLabel.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.Balance)}: {balance.Value}";
            }
            balanceLoader.StopAnimating();
        }

        public CustomAlertView Create(Post post, UINavigationController controller, UIView view)
        {
            _pickedCoin = CurrencyType.Steem;
            _coins = new List<CurrencyType>();
            switch (AppSettings.User.Chain)
            {
                case KnownChains.Steem:
                    _coins.AddRange(new[] { CurrencyType.Steem, CurrencyType.Sbd });
                    break;
                case KnownChains.Golos:
                    _coins.AddRange(new[] { CurrencyType.Golos, CurrencyType.Gbg });
                    break;
            }

            var popup = new UIView();
            popup.ClipsToBounds = true;
            popup.Layer.CornerRadius = 20;
            popup.BackgroundColor = Constants.R255G255B255;

            var _alert = new CustomAlertView(popup, controller);

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            popup.AutoSetDimension(ALDimension.Width, dialogWidth);

            var commonMargin = 20;

            var title = new UILabel();
            title.Font = Constants.Semibold14;
            title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
            title.TextAlignment = UITextAlignment.Center;
            popup.AddSubview(title);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            title.AutoSetDimension(ALDimension.Height, 70);

            var topSeparator = new UIView();
            topSeparator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(topSeparator);

            topSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title);
            topSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            topSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            topSeparator.AutoSetDimension(ALDimension.Height, 1);

            var container = new UIView();
            popup.AddSubview(container);

            container.AutoSetDimension(ALDimension.Height, 142);
            container.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, topSeparator);
            container.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            container.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);

            var promotionLabel = new UILabel();
            promotionLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Amount);
            promotionLabel.Font = Constants.Semibold14;

            container.AddSubview(promotionLabel);

            promotionLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 27);
            promotionLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            balanceLoader = new UIActivityIndicatorView();
            balanceLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            balanceLoader.Color = UIColor.Black;
            balanceLoader.HidesWhenStopped = true;
            balanceLoader.StartAnimating();

            container.AddSubview(balanceLoader);

            balanceLoader.AutoPinEdge(ALEdge.Left, ALEdge.Right, promotionLabel, 10);
            balanceLoader.AutoAlignAxis(ALAxis.Horizontal, promotionLabel);

            balanceLabel = new UILabel();
            balanceLabel.Font = Constants.Semibold14;
            balanceLabel.TextColor = Constants.R151G155B158;
            balanceLabel.TextAlignment = UITextAlignment.Right;

            container.AddSubview(balanceLabel);

            balanceLabel.AutoAlignAxis(ALAxis.Horizontal, promotionLabel);
            balanceLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            balanceLabel.AutoPinEdge(ALEdge.Left, ALEdge.Right, promotionLabel, 5);
            balanceLabel.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            GetBalance();

            var rightView = new UIView();
            container.AddSubview(rightView);
            rightView.AutoSetDimension(ALDimension.Height, 50);

            UIImageView pickerImage = new UIImageView(UIImage.FromBundle("ic_currency_picker.png"));
            rightView.AddSubview(pickerImage);
            pickerImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            pickerImage.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);

            UILabel _pickerLabel = new UILabel();
            _pickerLabel.Text = "STEEM";
            _pickerLabel.TextAlignment = UITextAlignment.Center;
            _pickerLabel.Font = Constants.Semibold14;
            _pickerLabel.TextColor = Constants.R255G71B5;
            rightView.AddSubview(_pickerLabel);
            _pickerLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _pickerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _pickerLabel.AutoPinEdge(ALEdge.Right, ALEdge.Left, pickerImage, -5);

            rightView.LayoutIfNeeded();

            _amountTextField = new SearchTextField(AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint),
                                                   new UIEdgeInsets(0, 20, 0, 0), new AmountFieldDelegate(), false, rightView);
            _amountTextField.KeyboardType = UIKeyboardType.DecimalPad;
            _amountTextField.Layer.CornerRadius = 25;
            container.AddSubview(_amountTextField);

            _amountTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _amountTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, promotionLabel, 16);
            _amountTextField.AutoSetDimension(ALDimension.Height, 50);

            errorMessage = new UILabel();
            errorMessage.Font = Constants.Semibold14;
            errorMessage.TextColor = Constants.R255G34B5;
            container.AddSubview(errorMessage);

            errorMessage.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _amountTextField);
            errorMessage.AutoPinEdge(ALEdge.Left, ALEdge.Left, _amountTextField);
            errorMessage.AutoPinEdge(ALEdge.Right, ALEdge.Right, _amountTextField);

            _amountTextField.EditingChanged += IsEnoughBalance;
            var max = new UIButton();
            max.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Max), UIControlState.Normal);
            max.SetTitleColor(UIColor.Black, UIControlState.Normal);
            max.Font = Constants.Semibold14;
            max.Layer.BorderWidth = 1;
            max.Layer.BorderColor = Constants.R245G245B245.CGColor;
            max.Layer.CornerRadius = 25;

            container.AddSubview(max);

            max.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            max.AutoPinEdge(ALEdge.Left, ALEdge.Right, _amountTextField, 10);
            max.AutoSetDimensionsToSize(new CGSize(80, 50));
            max.AutoAlignAxis(ALAxis.Horizontal, _amountTextField);
            max.TouchDown += MaxBtnOnClick;

            rightView.AutoAlignAxis(ALAxis.Horizontal, _amountTextField);
            rightView.AutoPinEdge(ALEdge.Right, ALEdge.Right, _amountTextField);
            container.BringSubviewToFront(rightView);

            UIPickerView picker = new UIPickerView();
            picker.Select(_coins.IndexOf(_pickedCoin), 0, true);
            picker.Model = new CoinPickerViewModel(_coins);
            picker.BackgroundColor = Constants.R255G255B255;
            popup.AddSubview(picker);

            picker.AutoMatchDimension(ALDimension.Height, ALDimension.Height, container);
            picker.AutoMatchDimension(ALDimension.Width, ALDimension.Width, container);
            picker.AutoPinEdge(ALEdge.Top, ALEdge.Top, container);
            var pickerHidden = picker.AutoPinEdge(ALEdge.Right, ALEdge.Left, container, -20);
            var pickerVisible = picker.AutoPinEdge(ALEdge.Right, ALEdge.Right, container);
            pickerVisible.Active = false;

            var promoteContainer = new UIView();
            promoteContainer.BackgroundColor = Constants.R255G255B255;
            popup.AddSubview(promoteContainer);

            promoteContainer.AutoMatchDimension(ALDimension.Height, ALDimension.Height, container);
            promoteContainer.AutoMatchDimension(ALDimension.Width, ALDimension.Width, container);
            promoteContainer.AutoPinEdge(ALEdge.Top, ALEdge.Top, container);
            var promoteHidden = promoteContainer.AutoPinEdge(ALEdge.Left, ALEdge.Right, container, 20);
            var promoteVisible = promoteContainer.AutoPinEdge(ALEdge.Left, ALEdge.Left, container);
            promoteVisible.Active = false;

            var avatar = new UIImageView();
            avatar.Layer.CornerRadius = 20;
            avatar.ClipsToBounds = true;
            promoteContainer.AddSubview(avatar);

            avatar.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 16);
            avatar.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            avatar.AutoSetDimensionsToSize(new CGSize(40, 40));

            var promoterLogin = new UILabel();
            promoterLogin.Font = Constants.Semibold14;
            promoterLogin.TextColor = Constants.R255G34B5;
            promoteContainer.AddSubview(promoterLogin);

            promoterLogin.AutoPinEdge(ALEdge.Left, ALEdge.Right, avatar, 20);
            promoterLogin.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            promoterLogin.AutoAlignAxis(ALAxis.Horizontal, avatar);

            var expectedTimeBackground = new UIView();
            expectedTimeBackground.Layer.CornerRadius = 10;
            expectedTimeBackground.BackgroundColor = Constants.R250G250B250;
            promoteContainer.AddSubview(expectedTimeBackground);

            expectedTimeBackground.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, avatar, 15);
            expectedTimeBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            expectedTimeBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            expectedTimeBackground.AutoSetDimension(ALDimension.Height, 50);

            var expectedTimeLabel = new UILabel();
            expectedTimeLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.ExpectedVoteTime);
            expectedTimeLabel.Font = Constants.Regular14;
            expectedTimeBackground.AddSubview(expectedTimeLabel);

            expectedTimeLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, DeviceHelper.IsSmallDevice ? 10 : 20);
            expectedTimeLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var expectedTimeValue = new UILabel();
            expectedTimeValue.TextAlignment = UITextAlignment.Right;
            expectedTimeValue.Font = Constants.Light20;
            expectedTimeBackground.AddSubview(expectedTimeValue);

            expectedTimeValue.AutoPinEdge(ALEdge.Left, ALEdge.Right, expectedTimeLabel);
            expectedTimeValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right, DeviceHelper.IsSmallDevice ? 10 : 20);
            expectedTimeValue.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var sureText = new UILabel();
            sureText.BackgroundColor = Constants.R255G255B255;
            sureText.Lines = 4;
            sureText.TextAlignment = UITextAlignment.Center;
            sureText.Font = Constants.Regular20;
            popup.AddSubview(sureText);

            sureText.AutoMatchDimension(ALDimension.Height, ALDimension.Height, container);
            sureText.AutoMatchDimension(ALDimension.Width, ALDimension.Width, container);
            sureText.AutoPinEdge(ALEdge.Top, ALEdge.Top, container);
            var sureTextHidden = sureText.AutoPinEdge(ALEdge.Left, ALEdge.Right, container, 20);
            var sureTextVisible = sureText.AutoPinEdge(ALEdge.Left, ALEdge.Left, container);
            sureTextVisible.Active = false;

            var completeText = new UILabel();
            completeText.BackgroundColor = Constants.R255G255B255;
            completeText.Lines = 4;
            completeText.TextAlignment = UITextAlignment.Center;
            completeText.Font = Constants.Regular20;
            popup.AddSubview(completeText);

            completeText.AutoMatchDimension(ALDimension.Height, ALDimension.Height, container);
            completeText.AutoMatchDimension(ALDimension.Width, ALDimension.Width, container);
            completeText.AutoPinEdge(ALEdge.Top, ALEdge.Top, container);
            var completeTextHidden = completeText.AutoPinEdge(ALEdge.Left, ALEdge.Right, container, 20);
            var completeTextVisible = completeText.AutoPinEdge(ALEdge.Left, ALEdge.Left, container);
            completeTextVisible.Active = false;

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, container);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            separator.AutoSetDimension(ALDimension.Height, 1);

            var selectButton = new UIButton();
            selectButton.SetTitle(string.Empty, UIControlState.Disabled);
            selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter), UIControlState.Normal);
            selectButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            selectButton.Layer.CornerRadius = 25;
            selectButton.Font = Constants.Bold14;
            popup.AddSubview(selectButton);

            selectButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            selectButton.AutoSetDimension(ALDimension.Height, 50);
            selectButton.LayoutIfNeeded();

            var loader = new UIActivityIndicatorView();
            loader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            loader.HidesWhenStopped = true;

            selectButton.AddSubview(loader);

            loader.AutoCenterInSuperview();

            var tap = new UITapGestureRecognizer(() =>
            {
                if (balances == null)
                    return;

                _amountTextField.ResignFirstResponder();
                pickerHidden.Active = false;
                pickerVisible.Active = true;

                UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                {
                    popup.LayoutIfNeeded();
                }, () =>
                {
                    title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SelectToken);
                    selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Select), UIControlState.Normal);
                });
            });

            rightView.AddGestureRecognizer(tap);

            var cancelButton = new UIButton();
            cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Close), UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            cancelButton.Layer.CornerRadius = 25;
            cancelButton.Font = Constants.Semibold14;
            cancelButton.Layer.BorderWidth = 1;
            cancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(cancelButton);

            cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, selectButton, 20);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
            cancelButton.AutoSetDimension(ALDimension.Height, 50);

            controller.View.EndEditing(true);

            Timer timer = null;

            OperationResult<PromoteResponse> promoter = null;

            selectButton.TouchDown += async (sender, e) =>
            {
                if (balanceLoader.IsAnimating)
                    return;

                IsEnoughBalance(null, null);

                if (pickerVisible.Active)
                {
                    _pickedCoin = _coins[(int)picker.SelectedRowInComponent(0)];
                    _pickerLabel.Text = _pickedCoin.ToString().ToUpper();

                    var balance = balances?.Find(x => x.CurrencyType == _pickedCoin);
                    balanceLabel.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.Balance)}: {balance?.Value}";

                    pickerHidden.Active = true;
                    pickerVisible.Active = false;
                    UIView.Animate(0.5, () =>
                    {
                        popup.LayoutIfNeeded();
                    }, () =>
                    {
                        title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                        selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter), UIControlState.Normal);
                        _amountTextField.UpdateRightViewRect();
                    });
                }
                else if (sureTextVisible.Active)
                {
                    if (!AppSettings.User.HasActivePermission)
                    {
                        _alert.Hidden = true;

                        controller.PushViewController(new LoginViewController(false), true);
                        return;
                    }

                    selectButton.Enabled = false;
                    loader.StartAnimating();

                    sureTextHidden.Active = true;
                    sureTextVisible.Active = false;

                    var transferResponse = await _presenter.TryTransferAsync(AppSettings.User.UserInfo, promoter.Result.Bot.Author, _amountTextField.GetDoubleValue().ToString(), _pickedCoin, $"https://steemit.com{post.Url}");

                    if (transferResponse.IsSuccess)
                        completeText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SuccessPromote);
                    else
                        completeText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TokenTransferError);

                    completeTextHidden.Active = false;
                    completeTextVisible.Active = true;

                    UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                    {
                        popup.LayoutIfNeeded();
                    }, () =>
                    {
                        selectButton.Enabled = true;
                        loader.StopAnimating();
                        title.Text = true ? AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoteComplete) : AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferError);
                        selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoteAgain), UIControlState.Normal);

                        promoteVisible.Active = false;
                        promoteHidden.Active = true;
                        popup.LayoutIfNeeded();
                        timer?.Dispose();
                    });
                }
                else if (promoteVisible.Active)
                {
                    var promoteConfirmation = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoteConfirmation,
                                                                                      _amountTextField.GetDoubleValue().ToString(),
                                                                                      _pickedCoin == CurrencyType.Sbd ? "SBD" : "Steem",
                                                                                      promoter.Result.Bot.Author);

                    sureText.Text = promoteConfirmation;
                    sureTextHidden.Active = false;
                    sureTextVisible.Active = true;

                    UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                    {
                        popup.LayoutIfNeeded();
                    }, () =>
                    {
                        selectButton.SetTitle("Yes", UIControlState.Normal);
                    });
                }
                else if (completeTextVisible.Active)
                {
                    completeTextHidden.Active = true;
                    completeTextVisible.Active = false;

                    UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                    {
                        popup.LayoutIfNeeded();
                    }, () =>
                    {
                        title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromotePost);
                        selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.FindPromoter), UIControlState.Normal);
                    });
                }
                else
                {
                    if (_amountTextField.GetDoubleValue() > balances?.Find(x => x.CurrencyType == _pickedCoin).Value)
                    {
                        errorMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NotEnoughBalance);
                        errorMessage.Hidden = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(_amountTextField.Text) || !IsValidAmount())
                        return;

                    selectButton.Enabled = false;
                    loader.StartAnimating();

                    var pr = new PromoteRequest()
                    {
                        Amount = _amountTextField.GetDoubleValue(),
                        CurrencyType = _pickedCoin,
                        PostToPromote = post,
                    };

                    promoter = await _presenter.FindPromoteBotAsync(pr);

                    if (promoter.IsSuccess)
                    {
                        var expectedUpvoteTime = promoter.Result.ExpectedUpvoteTime;
                        if (expectedUpvoteTime.ToString().Length > 8)
                            expectedTimeValue.Text = expectedUpvoteTime.ToString().Remove(8);
                        timer = new Timer((obj) =>
                        {
                            expectedUpvoteTime = expectedUpvoteTime.Subtract(TimeSpan.FromSeconds(1));
                            view.InvokeOnMainThread(() =>
                            {
                                if (expectedUpvoteTime.ToString().Length > 8)
                                    expectedTimeValue.Text = expectedUpvoteTime.ToString().Remove(8);
                                else
                                    expectedTimeValue.Text = expectedUpvoteTime.ToString();
                            });

                        }, null, DateTime.Now.Add(expectedUpvoteTime).Millisecond, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);

                        promoterLogin.Text = $"@{promoter.Result.Bot.Author}";

                        if (!string.IsNullOrEmpty(promoter.Result.Bot.Avatar))
                            ImageLoader.Load(promoter.Result.Bot.Avatar, avatar, size: new CGSize(300, 300));
                        else
                            avatar.Image = UIImage.FromBundle("ic_noavatar");

                        promoteHidden.Active = false;
                        promoteVisible.Active = true;
                        UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                        {
                            popup.LayoutIfNeeded();
                        }, () =>
                        {
                            selectButton.Enabled = true;
                            loader.StopAnimating();
                            title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoterFound);
                            selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Promote), UIControlState.Normal);
                        });
                    }
                    else
                    {
                        completeText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoterNotFound);

                        completeTextHidden.Active = false;
                        completeTextVisible.Active = true;

                        UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                        {
                            popup.LayoutIfNeeded();
                        }, () =>
                        {
                            selectButton.Enabled = true;
                            loader.StopAnimating();
                            title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PromoterSearchResult);
                            selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.SearchAgain), UIControlState.Normal);
                        });
                    }
                }
            };
            cancelButton.TouchDown += (sender, e) =>
            {
                _alert.Close();
            };

            var popuptap = new UITapGestureRecognizer(() =>
            {
                _amountTextField.ResignFirstResponder();
            });
            popup.AddGestureRecognizer(popuptap);

            Constants.CreateGradient(selectButton, 25);
            Constants.CreateShadowFromZeplin(selectButton, Constants.R231G72B0, 0.3f, 0, 10, 20, 0);
            popup.BringSubviewToFront(selectButton);

            _alert.Show();

            return _alert;
        }

        private void IsEnoughBalance(object sender, EventArgs e)
        {
            errorMessage.Hidden = true;
            var transferAmount = _amountTextField.GetDoubleValue();
            if (transferAmount == 0)
                return;

            if (transferAmount < Core.Constants.MinBid)
            {
                errorMessage.Hidden = false;
                errorMessage.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.MinBid)} {Core.Constants.MinBid}";
            }
            else if (transferAmount > Core.Constants.MaxBid)
            {
                errorMessage.Hidden = false;
                errorMessage.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.MaxBid)} {Core.Constants.MaxBid}";
            }
        }

        private bool IsValidAmount()
        {
            return _amountTextField.GetDoubleValue() >= 0.5 && _amountTextField.GetDoubleValue() <= 130;
        }

        private void MaxBtnOnClick(object sender, EventArgs e)
        {
            _amountTextField.Text = balances?.Find(x => x.CurrencyType == _pickedCoin).Value.ToString();
            IsEnoughBalance(null, null);
        }

        private void CoinSelected(CurrencyType pickedCoin)
        {
            _pickedCoin = pickedCoin;
        }
    }
}
