using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Errors;
using Steepshot.Core.Facades;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Extensions;
using Steepshot.iOS.ViewControllers;
using UIKit;
using System.Threading;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Helpers;
using System.Globalization;

namespace Steepshot.iOS.Views
{
    public partial class TransferViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        private readonly TransferFacade _transferFacade = new TransferFacade();
        private Timer _timer;
        private List<CurrencyType> _coins;
        private CurrencyType _pickedCoin;
        private string _prevQuery = string.Empty;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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
            if (_transferFacade.Recipient != null)
            {
                UIView.Animate(0.2, () =>
                {
                    _recipientAvatar.Hidden = false;
                    _recipientAvatar.LayoutIfNeeded();
                });
            }
            else
            {
                UIView.Animate(0.2, () =>
                {
                    _recipientAvatar.Hidden = true;
                    _recipientAvatar.LayoutIfNeeded();
                });
            }

            if (!string.IsNullOrEmpty(_transferFacade?.Recipient?.Avatar))
                ImageLoader.Load(_transferFacade.Recipient.Avatar, _recipientAvatar, size: new CGSize(40, 40), placeHolder: "ic_noavatar.png");
            else
                _recipientAvatar.Image = UIImage.FromBundle("ic_noavatar");
            _recepientTextField.Text = _transferFacade.Recipient.Author;
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
                _balance.Text = _transferFacade.UserBalance.Value.ToFormattedCurrencyString(_transferFacade.UserBalance.Precision, null, ".");
        }

        private void CoinSelected(CurrencyType pickedCoin)
        {
            _pickedCoin = pickedCoin;
            _pickerLabel.Text = _pickedCoin.ToString();
            _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?[_pickedCoin];
        }

        private async void Transfer(object sender, EventArgs e)
        {
            if (!AppSettings.User.HasActivePermission)
            {
                TabBarController.NavigationController.PushViewController(new LoginViewController(false), true);
                return;
            }

            if (string.IsNullOrEmpty(_recepientTextField.Text) || string.IsNullOrEmpty(_amountTextField.Text))
            {
                var transferResponse = await _presenter.TryTransfer(_transferFacade.Recipient.Author, double.Parse(_amountTextField.Text, CultureInfo.InvariantCulture), _pickedCoin, _transferFacade.UserBalance.ChainCurrency, _memoTextView.Text);
                if (transferResponse.IsSuccess)
                {
                    //ClearEdits();
                }
            }
        }

        private async void UpdateAccountInfo()
        {
            _balance.Hidden = true;
            _balanceLoader.StartAnimating();
            var response = await _transferFacade.TryGetAccountInfo(AppSettings.User.Login);
            if (response.IsSuccess)
            {
                AppSettings.User.AccountInfo = response.Result;
                _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?[_pickedCoin];
            }
            _balance.Hidden = false;
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

            if (!(searchResult is CanceledError))
            {
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
            _timer.Change(500, Timeout.Infinite);
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
                _tagsHorizontalAlignment.Constant = shift;
                _tagsNotFoundHorizontalAlignment.Constant = shift;
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
                _tagsNotFoundHorizontalAlignment.Constant = 0;
                _tagsHorizontalAlignment.Constant = 0;
                ScrollTheView(false);
            }
        }
    }

    public class CoinPickerViewModel : UIPickerViewModel
    {
        private readonly List<CurrencyType> _coins;

        public CoinPickerViewModel(List<CurrencyType> coins)
        {
            _coins = coins;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _coins.Count;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            var selected = row == pickerView.SelectedRowInComponent(component);
            var pickerLabel = new UILabel();
            pickerLabel.TextColor = UIColor.Black;
            pickerLabel.Text = _coins[(int)row].ToString();
            pickerLabel.Font = selected ? Constants.Regular27 : Constants.Light27;
            pickerLabel.TextAlignment = UITextAlignment.Center;
            pickerLabel.TextColor = selected ? UIColor.Red : UIColor.Black;
            return pickerLabel;
        }

        [Export("pickerView:didSelectRow:inComponent:")]
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            pickerView.ReloadComponent(0);
        }
    }
}
