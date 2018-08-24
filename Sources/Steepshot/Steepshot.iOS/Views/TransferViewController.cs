using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Facades;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.iOS.ViewControllers;
using UIKit;
using System.Threading;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Helpers;
using System.Globalization;
using Steepshot.Core.Localization;

namespace Steepshot.iOS.Views
{
    public partial class TransferViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        private TransferFacade _transferFacade;
        private Timer _timer;
        private List<CurrencyType> _coins;
        private CurrencyType _pickedCoin;
        private string _prevQuery = string.Empty;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var client = AppDelegate.MainChain == KnownChains.Steem ? AppDelegate.SteemClient : AppDelegate.GolosClient;
            _transferFacade = new TransferFacade();
            _transferFacade.SetClient(client);
            _transferFacade.OnRecipientChanged += OnRecipientChanged;
            _transferFacade.OnUserBalanceChanged += OnUserBalanceChanged;
            _transferFacade.UserFriendPresenter.SourceChanged += PresenterOnSourceChanged;

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
            _timer = new Timer(OnTimer);

            CreateView();

            Activeview = _memoTextView;

            SetBackButton();
            SetPlaceholder();

            CoinSelected(CurrencyType.Steem);
            UpdateAccountInfo();
        }

        public override void ViewDidLayoutSubviews()
        {
            Constants.CreateGradient(_transferButton, 25);
        }

        private void PresenterOnSourceChanged(Status obj)
        {
            _usersTable.ReloadData();
        }

        private void OnRecipientChanged()
        {
            var isRecipientSetted = _transferFacade?.Recipient == null;

            if (_recipientAvatar.Hidden != isRecipientSetted)
            {
                if (!string.IsNullOrEmpty(_transferFacade?.Recipient?.Author))
                    _recepientTextField.Text = _transferFacade.Recipient.Author;
                UIView.Animate(0.2, () =>
                {
                    _recipientAvatar.Hidden = isRecipientSetted;
                    _recipientAvatar.LayoutIfNeeded();
                });
            }

            if (!string.IsNullOrEmpty(_transferFacade?.Recipient?.Avatar))
                ImageLoader.Load(_transferFacade?.Recipient?.Avatar, _recipientAvatar, size: new CGSize(40, 40), placeHolder: "ic_noavatar.png");
            else
                _recipientAvatar.Image = UIImage.FromBundle("ic_noavatar");
        }

        private void CellAction(ActionType type, UserFriend recipient)
        {
            _transferFacade.Recipient = recipient;
            RemoveFocus();
        }

        private void OnTimer(object state)
        {
            InvokeOnMainThread(() =>
            {
                LoadUsers(true, true);
            });
        }

        public void GetItems()
        {
            LoadUsers(false, false);
        }

        protected override void ScrollTheView(bool move)
        {
            if (_memoTextView.IsFirstResponder)
                base.ScrollTheView(move);
            else
            {
                if (move)
                    _usersTable.ScrollIndicatorInsets = _usersTable.ContentInset = new UIEdgeInsets(0, 0, _tableScrollAmount, 0);
                else
                    _usersTable.ScrollIndicatorInsets = _usersTable.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            }
        }

        private void OnUserBalanceChanged()
        {
            if (_transferFacade.UserBalance != null)
                _balanceLabel.Text = $"{AppSettings.LocalizationManager.GetText(LocalizationKeys.Balance)}: {_transferFacade.UserBalance.Value}";
        }

        private void CoinSelected(CurrencyType pickedCoin)
        {
            _pickedCoin = pickedCoin;
            _pickerLabel.Text = _pickedCoin.ToString().ToUpper();
            _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?.First(b => b.CurrencyType == pickedCoin);
            IsEnoughBalance(null, null);
        }

        private void TogglButtons(bool enabled)
        {
            _transferButton.Enabled = enabled;
            _recepientTextField.Enabled = enabled;
            _amountTextField.Enabled = enabled;
            _memoTextView.UserInteractionEnabled = enabled;
            memoLabel.UserInteractionEnabled = enabled;
        }

        private void Transfer(object sender, EventArgs e)
        {
            if (_transferFacade.UserBalance == null || _transferFacade.Recipient == null)
            {
                ShowAlert(LocalizationKeys.WrongRecipientName);
                return;
            }

            var transferAmount = _amountTextField.GetDoubleValue();

            if (Math.Abs(transferAmount) < 0.00000001 || transferAmount > _transferFacade.UserBalance.Value)
                return;

            if (!AppSettings.User.HasActivePermission)
            {
                NavigationController.PushViewController(new LoginViewController(false), true);
                return;
            }

            Popups.TransferDialogPopup.Create(NavigationController,
                                              _transferFacade.Recipient.Author,
                                              _amountTextField.GetDoubleValue().ToString(),
                                              _pickedCoin,
                                              ContinueTransfer
                                             );
        }

        private async void ContinueTransfer(bool shouldContinue)
        {
            if (shouldContinue)
            {
                TogglButtons(false);

                _tranfserLoader.StartAnimating();
                RemoveFocus();

                var transferResponse = await _presenter.TryTransfer(AppSettings.User.UserInfo, _transferFacade.Recipient.Author, _amountTextField.GetDoubleValue().ToString(), _pickedCoin, _memoTextView.Text);

                _tranfserLoader.StopAnimating();
                TogglButtons(true);

                if (transferResponse.IsSuccess)
                {
                    ShowSuccessPopUp();
                    _transferFacade.UserFriendPresenter.Clear();
                    _userTableSource.ClearPosition();
                    _recepientTextField.Clear();
                    _amountTextField.Clear();
                    _transferFacade.Recipient = null;
                    _memoTextView.Text = string.Empty;
                }
                else
                    ShowAlert(transferResponse.Exception);
            }
        }

        private async void UpdateAccountInfo()
        {
            _balanceLabel.TextColor = UIColor.Clear;
            _balanceLoader.StartAnimating();
            var response = await _transferFacade.TryGetAccountInfo(AppSettings.User.Login);
            if (response.IsSuccess)
            {
                AppSettings.User.AccountInfo = response.Result;
                _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?.First(b => b.CurrencyType == _pickedCoin);
            }
            _balanceLabel.TextColor = Constants.R151G155B158;
            _balanceLoader.StopAnimating();
        }

        private async void LoadUsers(bool clear, bool isLoaderNeeded = true)
        {
            if (_recepientTextField.Text == _transferFacade?.Recipient?.Author || !_recepientTextField.IsFirstResponder)
                return;

            if (clear)
            {
                _transferFacade.UserFriendPresenter.Clear();
                _userTableSource.ClearPosition();
                _prevQuery = _recepientTextField.Text;
            }

            if (isLoaderNeeded)
            {
                _noResultViewTags.Hidden = true;
                _usersLoader.StartAnimating();
            }
            var searchResult = await _transferFacade.TryLoadNextSearchUser(_recepientTextField.Text);

            if (!(searchResult is OperationCanceledException))
            {
                if (_recepientTextField.IsFirstResponder)
                    _noResultViewTags.Hidden = _transferFacade.UserFriendPresenter.Count > 0;
                _usersLoader.StopAnimating();

                if (!_isWarningOpen && searchResult != null)
                {
                    UIView.Animate(0.3f, 0f, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _isWarningOpen = true;
                        warningViewToBottomConstraint.Constant = -_tableScrollAmount - 20;
                        warningView.Alpha = 1;
                        View.LayoutIfNeeded();
                    }, () =>
                    {
                        UIView.Animate(0.2f, 5f, UIViewAnimationOptions.CurveEaseIn, () =>
                        {
                            warningViewToBottomConstraint.Constant = -_tableScrollAmount + 60;
                            warningView.Alpha = 0;
                            View.LayoutIfNeeded();
                        }, () =>
                        {
                            _isWarningOpen = false;
                        });
                    });
                }
            }
        }

        private void EditingChanged(object sender, EventArgs e)
        {
            _timer.Change(1300, Timeout.Infinite);
        }

        private void RemoveFocus()
        {
            _recepientTextField.ResignFirstResponder();
            _amountTextField.ResignFirstResponder();
            _memoTextView.ResignFirstResponder();
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            if (_memoTextView.IsFirstResponder)
            {
                base.KeyBoardUpNotification(notification);
            }
            else
            {
                var shift = -90;
                _userLoaderHorizontalAlignment.Constant = shift;
                _usersNotFoundHorizontalAlignment.Constant = shift;
                warningView.Hidden = false;

                if (_tableScrollAmount == 0)
                {
                    var r = UIKeyboard.FrameEndFromNotification(notification);
                    _tableScrollAmount = DeviceHelper.GetVersion() == DeviceHelper.HardwareVersion.iPhoneX ? r.Height - 34 : r.Height;
                    warningViewToBottomConstraint.Constant = -_tableScrollAmount + 60;
                }
                ScrollTheView(true);
            }
        }

        protected override void CalculateBottom()
        {
            var absolutePosition = _memoTextView.ConvertRectToView(_memoTextView.Frame, View);
            Bottom = (absolutePosition.Y + absolutePosition.Height + Offset);
        }

        protected override void KeyBoardDownNotification(NSNotification notification)
        {
            if (_memoTextView.IsFirstResponder)
            {
                base.KeyBoardDownNotification(notification);
            }
            else
            {
                warningView.Hidden = true;
                warningViewToBottomConstraint.Constant = -_tableScrollAmount + 60;
                _usersNotFoundHorizontalAlignment.Constant = 0;
                _userLoaderHorizontalAlignment.Constant = 0;
                ScrollTheView(false);
            }
        }
    }
}
