using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;
using static Steepshot.Core.Clients.BaseServerClient;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.ViewControllers
{
    public abstract class BasePostController<T> : BaseViewControllerWithPresenter<T> where T : BasePostPresenter, new()
    {
        private UIView dialog;
        private UIButton rightButton;

        protected async void Vote(Post post)
        {
            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped(null, null);
                return;
            }

            if (post == null)
                return;

            var exception = await _presenter.TryVote(post);
            if (exception is OperationCanceledException)
                return;

            ShowAlert(exception);
            if (exception == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        protected virtual async void LoginTapped(object sender, EventArgs e)
        {
            var response = await _presenter.CheckServiceStatus();

            var myViewController = new WelcomeViewController(response.IsSuccess);
            NavigationController.PushViewController(myViewController, true);
        }

        protected void Flagged(Post post, List<UIAlertAction> actions = null)
        {
            var actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            if (actions != null)
                foreach (var action in actions)
                    actionSheetAlert.AddAction(action);
            if (post.Author == AppSettings.User.Login)
            {
                if (post.CashoutTime > post.Created)
                {
                    actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.EditPost), UIAlertActionStyle.Default, obj => EditPost(post)));

                    actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.DeletePost), UIAlertActionStyle.Default, obj => DeleteAlert(post)));
                }
            }
            else
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.FlagPhoto), UIAlertActionStyle.Default, obj => FlagPhoto(post)));
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.HidePhoto), UIAlertActionStyle.Default, obj => HidePhoto(post)));
            }
            actionSheetAlert.AddAction(UIAlertAction.Create("Promote", UIAlertActionStyle.Default, obj => PromotePost(post)));
            //Sharepost contain copylink function by default
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Sharepost), UIAlertActionStyle.Default, obj => SharePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private List<CurrencyType> _coins;
        private CurrencyType _pickedCoin = CurrencyType.Steem;
        private SearchTextField _amountTextField;

        UIActivityIndicatorView balanceLoader;
        UILabel balanceLabel;
        UILabel errorMessage;

        List<BalanceModel> balances;

        private async void GetBalance()
        {
            balanceLoader.StartAnimating();
            var response = await _presenter.TryGetAccountInfo(AppSettings.User.Login);

            if (response.IsSuccess)
            {
                balances = response.Result?.Balances;
                var balance = balances?.Find(x => x.CurrencyType == _pickedCoin);
                balanceLabel.Text = $"Balance: {balance.Value}";
            }
            balanceLoader.StopAnimating();
        }

        public override void ViewDidAppear(bool animated)
        {
            if(_alert != null)
                _alert.Hidden = false;

            base.ViewDidAppear(animated);
        }

        CustomAlertView _alert;

        protected void PromotePost(Post post)
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

            _alert = new CustomAlertView(popup, TabBarController.NavigationController);

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            popup.AutoSetDimension(ALDimension.Width, dialogWidth);

            var commonMargin = 20;

            var title = new UILabel();
            title.Font = Constants.Semibold14;
            title.Text = "Promote post";
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
            promotionLabel.Text = "Amount";
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

            var amountTextFieldDelegate = new AmountFieldDelegate();
            _amountTextField = new SearchTextField(AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint),
                                                   new UIEdgeInsets(0, 20, 0, 0), amountTextFieldDelegate, false, rightView);
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

            _amountTextField.EditingChanged += 汤;
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
            expectedTimeLabel.Text = "Expected vote time";
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
            selectButton.SetTitle("FIND PROMOTER", UIControlState.Normal);
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

            NavigationController.View.EndEditing(true);

            Timer timer = null;

            PromoteResponse promoter = null;

            selectButton.TouchDown += async (sender, e) =>
            {
                if (balanceLoader.IsAnimating)
                    return;

                汤(null, null);

                if (pickerVisible.Active)
                {
                    _pickedCoin = _coins[(int)picker.SelectedRowInComponent(0)];
                    _pickerLabel.Text = _pickedCoin.ToString().ToUpper();

                    var balance = balances?.Find(x => x.CurrencyType == _pickedCoin);
                    balanceLabel.Text = $"Balance: {balance.Value}";

                    pickerHidden.Active = true;
                    pickerVisible.Active = false;
                    UIView.Animate(0.5, () =>
                    {
                        popup.LayoutIfNeeded();
                    }, () =>
                    {
                        title.Text = "Promote post";
                        selectButton.SetTitle("FIND PROMOTER", UIControlState.Normal);
                        _amountTextField.UpdateRightViewRect();
                    });
                }
                else if (promoteVisible.Active)
                {
                    if (!AppSettings.User.HasActivePermission)
                    {
                        _alert.Hidden = true;

                        TabBarController.NavigationController.PushViewController(new LoginViewController(false), true);
                        return;
                    }

                    selectButton.Enabled = false;
                    loader.StartAnimating();

                    var transferResponse = await _presenter.TryTransfer(AppSettings.User.UserInfo, promoter.Bot.Author, _amountTextField.GetDoubleValue().ToString(), _pickedCoin, $"https://steemit.com{post.Url}");

                    if (transferResponse.IsSuccess)
                        completeText.Text = "Your bid has been successfully sent. Wait for upvote.";
                    else
                        completeText.Text = "Tokens transfer error";

                    completeTextHidden.Active = false;
                    completeTextVisible.Active = true;

                    UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                    {
                        popup.LayoutIfNeeded();
                    }, () =>
                    {
                        selectButton.Enabled = true;
                        loader.StopAnimating();
                        title.Text = true ? "Promote is complete" : "Transfer error";
                        selectButton.SetTitle("Promote again???", UIControlState.Normal);

                        promoteVisible.Active = false;
                        promoteHidden.Active = true;
                        popup.LayoutIfNeeded();
                        timer?.Dispose();
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
                        title.Text = "Promote post";
                        selectButton.SetTitle("FIND PROMOTER", UIControlState.Normal);
                    });
                }
                else
                {
                    if (_amountTextField.GetDoubleValue() > balances?.Find(x => x.CurrencyType == _pickedCoin).Value)
                    {
                        errorMessage.Text = "Not enough balance";
                        errorMessage.Hidden = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(_amountTextField.Text) || !IsValidAmount())
                        return;

                    selectButton.Enabled = false;
                    loader.StartAnimating();

                    var pr = new Core.Clients.BaseServerClient.PromoteRequest()
                    {
                        Amount = _amountTextField.GetDoubleValue(),
                        CurrencyType = _pickedCoin,
                        PostToPromote = post,
                    };

                    promoter = await _presenter.FindPromoteBot(pr);

                    if (promoter != null)
                    {
                        var expectedUpvoteTime = promoter.ExpectedUpvoteTime;
                        if(expectedUpvoteTime.ToString().Length > 8)
                            expectedTimeValue.Text = expectedUpvoteTime.ToString().Remove(8);
                        timer = new Timer((obj) =>
                        {
                            expectedUpvoteTime = expectedUpvoteTime.Subtract(TimeSpan.FromSeconds(1));
                            InvokeOnMainThread(() =>
                            {
                                if (expectedUpvoteTime.ToString().Length > 8)
                                    expectedTimeValue.Text = expectedUpvoteTime.ToString().Remove(8);
                                else
                                    expectedTimeValue.Text = expectedUpvoteTime.ToString();
                            });

                        }, null, DateTime.Now.Add(expectedUpvoteTime).Millisecond, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);


                        promoterLogin.Text = $"@{promoter.Bot.Author}";

                        if (!string.IsNullOrEmpty(promoter.Bot.Avatar))
                            ImageLoader.Load(promoter.Bot.Avatar, avatar, size: new CGSize(300, 300));
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
                            title.Text = "Promoter found";
                            selectButton.SetTitle("PROMOTE", UIControlState.Normal);
                        });
                    }
                    else
                    {
                        completeText.Text = "We look, but there's no appropriate bot for promotion. Please, try a little later.";

                        completeTextHidden.Active = false;
                        completeTextVisible.Active = true;

                        UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                        {
                            popup.LayoutIfNeeded();
                        }, () =>
                        {
                            selectButton.Enabled = true;
                            loader.StopAnimating();
                            title.Text = "Promoter search result";
                            selectButton.SetTitle("Search again", UIControlState.Normal);
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
        }

        private void 汤(object sender, EventArgs e)
        {
            errorMessage.Hidden = true;
            var transferAmount = _amountTextField.GetDoubleValue();
            if (transferAmount == 0)
                return;

            if (transferAmount < 0.5)
            {
                errorMessage.Hidden = false;
                errorMessage.Text = "Min bid is 0.5";
            }
            else if (transferAmount > 130)
            {
                errorMessage.Hidden = false;
                errorMessage.Text = "Max bid is 100";
            }
        }

        private bool IsValidAmount()
        {
            return _amountTextField.GetDoubleValue() >= 0.5 && _amountTextField.GetDoubleValue() <= 130;
        }

        private void MaxBtnOnClick(object sender, EventArgs e)
        {
            _amountTextField.Text = balances?.Find(x => x.CurrencyType == _pickedCoin).Value.ToString();
            汤(null, null);
        }

        private void CoinSelected(CurrencyType pickedCoin)
        {
            _pickedCoin = pickedCoin;
        }

        protected void HidePhoto(Post post)
        {
            AppSettings.User.PostBlackList.Add(post.Url);
            AppSettings.User.Save();

            _presenter.HidePost(post);
        }

        protected async Task FlagPhoto(Post post)
        {
            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped(null, null);
                return;
            }

            if (post == null)
                return;

            var exception = await _presenter.TryFlag(post);
            ShowAlert(exception);
            if (exception == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        private void CopyLink(Post post)
        {
            UIPasteboard.General.String = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostLink, post.Url);
            ShowAlert(LocalizationKeys.Copied);
        }

        private void SharePhoto(Post post)
        {
            var postLink = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostLink, post.Url);
            var item = NSObject.FromObject(postLink);
            var activityItems = new NSObject[] { item };

            var activityController = new UIActivityViewController(activityItems, null);
            PresentViewController(activityController, true, null);
        }

        private void DeleteAlert(Post post)
        {
            CustomAlertView _alert = null;

            if (_alert == null)
            {
                var titleText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertTitle);
                var messageText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertMessage);
                var leftButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
                var rightButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Delete);

                var commonMargin = 20;
                var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;

                dialog = new UIView();
                dialog.ClipsToBounds = true;
                dialog.Layer.CornerRadius = 15;
                dialog.BackgroundColor = UIColor.White;

                dialog.AutoSetDimension(ALDimension.Width, dialogWidth);

                // Title

                var title = new UILabel();
                title.Lines = 3;
                title.LineBreakMode = UILineBreakMode.WordWrap;
                title.UserInteractionEnabled = false;
                title.Font = Constants.Regular20;
                title.TextAlignment = UITextAlignment.Center;
                title.Text = titleText;
                title.BackgroundColor = UIColor.Clear;
                dialog.AddSubview(title);

                title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 24);
                title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 10);
                title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);

                var size = title.SizeThatFits(new CGSize(dialogWidth - commonMargin * 2, 0));
                title.AutoSetDimension(ALDimension.Height, size.Height);

                // Alert message

                var message = new UILabel();
                message.Lines = 9;
                message.LineBreakMode = UILineBreakMode.WordWrap;
                message.UserInteractionEnabled = false;
                message.Font = Constants.Regular14;
                message.TextAlignment = UITextAlignment.Center;
                message.Text = messageText;
                message.BackgroundColor = UIColor.Clear;
                dialog.AddSubview(message);

                message.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 22);
                message.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 10);
                message.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);

                size = message.SizeThatFits(new CGSize(dialogWidth - commonMargin * 2, 0));
                message.AutoSetDimension(ALDimension.Height, size.Height);

                // Separator

                var separator = new UIView();
                separator.BackgroundColor = Constants.R245G245B245;
                dialog.AddSubview(separator);

                separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, message, 26);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                separator.AutoSetDimension(ALDimension.Height, 1);

                var leftButton = CreateButton(leftButtonText, UIColor.Black);
                leftButton.Font = Constants.Semibold14;
                leftButton.Layer.BorderWidth = 1;
                leftButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
                dialog.AddSubview(leftButton);

                leftButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
                leftButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                leftButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
                leftButton.AutoSetDimension(ALDimension.Width, dialogWidth / 2 - 27);
                leftButton.AutoSetDimension(ALDimension.Height, 50);

                rightButton = CreateButton(rightButtonText, UIColor.White);
                rightButton.Font = Constants.Bold14;
                dialog.AddSubview(rightButton);

                rightButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
                rightButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, leftButton, 15);
                rightButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                rightButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
                rightButton.AutoSetDimension(ALDimension.Width, dialogWidth / 2 - 27);
                rightButton.AutoSetDimension(ALDimension.Height, 50);
                rightButton.LayoutIfNeeded();

                NavigationController.View.EndEditing(true);

                _alert = new CustomAlertView(dialog, TabBarController);

                leftButton.TouchDown += (sender, e) => { _alert.Close(); };
                rightButton.TouchDown += (sender, e) => { DeletePost(post, _alert.Close); };

                Constants.CreateGradient(rightButton, 25);
                Constants.CreateShadow(rightButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
            }
            _alert.Show();
        }

        private async void DeletePost(Post post, Action action)
        {
            action.Invoke();

            var exception = await _presenter.TryDeletePost(post);

            if (exception != null)
                ShowAlert(exception);
        }

        private void EditPost(Post post)
        {
            var editPostViewController = new PostEditViewController(post);
            TabBarController.NavigationController.PushViewController(editPostViewController, true);
        }

        public UIButton CreateButton(string title, UIColor titleColor)
        {
            var button = new UIButton();
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(titleColor, UIControlState.Normal);
            button.Layer.CornerRadius = 25;

            return button;
        }

        protected abstract void SameTabTapped();
        protected abstract Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false);
        protected abstract void SourceChanged(Status status);

        protected async void ScrolledToBottom()
        {
            await GetPosts(false, false);
        }

        protected void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }

        protected sealed override void CreatePresenter()
        {
            base.CreatePresenter();
            _presenter.SourceChanged += SourceChanged;
        }
    }
}
