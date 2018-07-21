using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core;
using Steepshot.Core.Facades;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Steepshot.iOS.ViewSources;
using Steepshot.iOS.Cells;
using System.Threading;
using Steepshot.Core.Models;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Common;

namespace Steepshot.iOS.Views
{
    public class TransferViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        private UIButton _transferButton = new UIButton();
        private UILabel _pickerLabel = new UILabel();
        private SearchTextField _recepientTextField;
        private SearchTextField _amountTextField;
        private readonly TransferFacade _transferFacade = new TransferFacade();
        private Timer _timer;
        private List<CurrencyType> _coins;
        private CurrencyType _pickedCoin;
        private UITableView _usersTable;
        private FollowTableViewSource _userTableSource;
        private string _prevQuery = string.Empty;
        private UIActivityIndicatorView _usersLoader;
        private NSLayoutConstraint _tagsHorizontalAlignment;
        private NSLayoutConstraint _tagsNotFoundHorizontalAlignment;
        private NSLayoutConstraint warningViewToBottomConstraint;
        private bool _isWarningOpen;
        private UIView warningView;
        private UILabel _noResultViewTags = new UILabel();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //_transferFacade.OnRecipientChanged += OnRecipientChanged;
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

            SetBackButton();
            CoinSelected(CurrencyType.Steem);
            UpdateAccountInfo();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(_transferButton, 25);
        }

        private void PresenterOnSourceChanged(Status obj)
        {
            _usersTable.ReloadData();
            //_recipientSearchLoader.Visibility = ViewStates.Gone;
            //_emptyQueryLabel.Visibility = _transferFacade.UserFriendPresenter.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
            //if (State == FragmentState.TransferPrepare)
            //_transferFacade.Recipient = _transferFacade.Recipient ?? _transferFacade.UserFriendPresenter.FirstOrDefault(recipient => recipient.Author.Equals(_recipientSearch.Text));
        }

        private void CellAction(ActionType type, UserFriend recipient)
        {
            _transferFacade.Recipient = recipient;
            RemoveFocus();
            //_transferBtn.RequestFocus();
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

        private void SetupTable()
        {
            _usersTable = new UITableView();
            _usersTable.Hidden = true;
            View.AddSubview(_usersTable);

            _usersTable.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _recepientTextField, 25);
            _usersTable.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _usersTable.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _usersTable.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _userTableSource = new FollowTableViewSource(_transferFacade.UserFriendPresenter, _usersTable);
            _userTableSource.ScrolledToBottom += GetItems;
            _userTableSource.CellAction += CellAction;
            _usersTable.Source = _userTableSource;
            _usersTable.AllowsSelection = false;
            _usersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            _usersTable.LayoutMargins = UIEdgeInsets.Zero;
            _usersTable.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            _usersTable.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));
            _usersTable.RegisterClassForCellReuse(typeof(LoaderCell), nameof(LoaderCell));
            _usersTable.RowHeight = 70f;

            _usersLoader = new UIActivityIndicatorView();
            _usersLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _usersLoader.Color = Constants.R231G72B0;
            _usersLoader.HidesWhenStopped = true;
            _usersLoader.StopAnimating();
            View.AddSubview(_usersLoader);

            _tagsHorizontalAlignment = _usersLoader.AutoAlignAxis(ALAxis.Horizontal, _usersTable);
            _usersLoader.AutoAlignAxis(ALAxis.Vertical, _usersTable);
        }

        protected override void ScrollTheView(bool move)
        {
            if (move)
                _usersTable.ScrollIndicatorInsets = _usersTable.ContentInset = new UIEdgeInsets(0, 0, ScrollAmount, 0);
            else
                _usersTable.ScrollIndicatorInsets = _usersTable.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
        }

        private void OnUserBalanceChanged()
        {
            if (_transferFacade.UserBalance != null)
                _balance.Text = $"{_transferFacade.UserBalance.Value} {_pickedCoin.ToString()}";
        }

        private void CoinSelected(CurrencyType pickedCoin)
        {
            _pickedCoin = pickedCoin;
            _pickerLabel.Text = _pickedCoin.ToString();
            _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?.First(b => b.CurrencyType == pickedCoin);
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
                //All fields should be filled
                return;
            }
            /*
            var response = await _presenter.TryTransfer(_recepientTextField.Text, double.Parse(_amountTextField.Text), Core.Models.Requests.CurrencyType.Steem);
            if (response.IsSuccess)
            {
                return;
            }
            */
        }

        private UIActivityIndicatorView _balanceLoader = new UIActivityIndicatorView();
        private UILabel _balance;

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = "Transfer";

            _balance = new UILabel()
            {
                Font = Constants.Semibold20,
                TextColor = Constants.R151G155B158,
                TextAlignment = UITextAlignment.Right,
            };

            _balanceLoader = new UIActivityIndicatorView();
            _balanceLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            _balanceLoader.Color = Constants.R231G72B0;
            _balanceLoader.HidesWhenStopped = true;
            _balance.AddSubview(_balanceLoader);

            _balanceLoader.AutoCenterInSuperview();

            var rightBarButton = new UIBarButtonItem(_balance);
            NavigationItem.RightBarButtonItem = rightBarButton;

            _balance.AutoSetDimension(ALDimension.Width, 100);
        }

        private async void UpdateAccountInfo()
        {
            _balance.Hidden = true;
            _balanceLoader.StartAnimating();
            var response = await _transferFacade.TryGetAccountInfo(AppSettings.User.Login);
            if (response.IsSuccess)
            {
                AppSettings.User.AccountInfo = response.Result;
                _transferFacade.UserBalance = AppSettings.User.AccountInfo?.Balances?.First(b => b.CurrencyType == _pickedCoin);
            }
            _balance.Hidden = false;
            _balanceLoader.StopAnimating();
        }

        private async void LoadUsers(bool clear, bool isLoaderNeeded = true)
        {
            if (_recepientTextField.Text == _transferFacade?.Recipient?.Author)
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
                _noResultViewTags.Hidden = _transferFacade.UserFriendPresenter.Count > 0;
                _usersLoader.StopAnimating();

                if (!_isWarningOpen && searchResult != null)
                {
                    UIView.Animate(0.3f, 0f, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _isWarningOpen = true;
                        warningViewToBottomConstraint.Constant = -ScrollAmount - 20;
                        warningView.Alpha = 1;
                        View.LayoutIfNeeded();
                    }, () =>
                    {
                        UIView.Animate(0.2f, 5f, UIViewAnimationOptions.CurveEaseIn, () =>
                        {
                            warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
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

        private void CreateView()
        {
            View.BackgroundColor = UIColor.White;

            UILabel recepientLabel = new UILabel();
            recepientLabel.Text = "Recipient name";
            recepientLabel.Font = Constants.Semibold14;
            recepientLabel.TextColor = UIColor.Black;
            View.AddSubview(recepientLabel);

            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 25);
            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);

            _recepientTextField = new SearchTextField(() =>
            {
                _recepientTextField.ResignFirstResponder();
            }, "Search");
            _recepientTextField.EditingChanged += EditingChanged;
            _recepientTextField.Layer.CornerRadius = 25;
            View.AddSubview(_recepientTextField);

            _recepientTextField.ClearButtonTapped += () =>
            {
                _transferFacade.UserFriendPresenter.Clear();
                //_usersTable.ReloadData();
            };

            View.AddSubview(_recepientTextField);

            _recepientTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _recepientTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recepientLabel, 16);
            _recepientTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            _recepientTextField.AutoSetDimension(ALDimension.Height, 50);

            UILabel amountLabel = new UILabel();
            amountLabel.Text = "Transfer amount";
            amountLabel.Font = Constants.Semibold14;
            amountLabel.TextColor = UIColor.Black;
            View.AddSubview(amountLabel);

            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            amountLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _recepientTextField, 25);
            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);

            _amountTextField = new SearchTextField(() =>
            {

            }, "0");
            _amountTextField.KeyboardType = UIKeyboardType.NumbersAndPunctuation;
            _amountTextField.Layer.CornerRadius = 25;
            View.AddSubview(_amountTextField);

            _amountTextField.ClearButtonTapped += () => { };

            View.AddSubview(_amountTextField);

            _amountTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _amountTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountLabel, 16);
            _amountTextField.AutoSetDimension(ALDimension.Height, 50);

            UIView pickerView = new UIView();
            pickerView.Layer.CornerRadius = 25;
            pickerView.Layer.BorderColor = Constants.R244G244B246.CGColor;
            pickerView.Layer.BorderWidth = 1;
            pickerView.UserInteractionEnabled = true;
            var pickerTap = new UITapGestureRecognizer(() =>
            {
                UIView popup = new UIView();
                popup.ClipsToBounds = true;
                popup.Layer.CornerRadius = 15;
                popup.BackgroundColor = UIColor.White;
                View.AddSubview(popup);
                var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
                popup.AutoSetDimension(ALDimension.Width, dialogWidth);

                var commonMargin = 20;

                var title = new UILabel();
                title.Font = Constants.Semibold14;
                title.Text = "Select token";
                title.SizeToFit();
                popup.AddSubview(title);
                title.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
                title.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
                title.AutoSetDimension(ALDimension.Height, 70);

                UIPickerView picker = new UIPickerView();
                picker.Model = new CoinPickerViewModel(_coins);
                popup.AddSubview(picker);

                picker.AutoSetDimension(ALDimension.Width, dialogWidth - commonMargin * 2);
                picker.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title);
                picker.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
                picker.SizeToFit();

                var topSeparator = new UIView();
                topSeparator.BackgroundColor = Constants.R245G245B245;
                popup.AddSubview(topSeparator);

                topSeparator.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, picker);
                topSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                topSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                topSeparator.AutoSetDimension(ALDimension.Height, 1);

                //var titleText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertTitle);
                //var messageText = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertMessage);
                //var leftButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
                //var rightButtonText = AppSettings.LocalizationManager.GetText(LocalizationKeys.Delete);

                var separator = new UIView();
                separator.BackgroundColor = Constants.R245G245B245;
                popup.AddSubview(separator);

                separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, picker);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                separator.AutoSetDimension(ALDimension.Height, 1);

                var selectButton = new UIButton();
                selectButton.SetTitle("SELECT", UIControlState.Normal);
                selectButton.SetTitleColor(UIColor.White, UIControlState.Normal);
                selectButton.Layer.CornerRadius = 25;
                selectButton.Font = Constants.Bold14;
                popup.AddSubview(selectButton);

                selectButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
                selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                selectButton.AutoSetDimension(ALDimension.Height, 50);
                selectButton.LayoutIfNeeded();

                var cancelButton = new UIButton();
                cancelButton.SetTitle("CLOSE", UIControlState.Normal);
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

                var alert = new CustomAlertView(popup, TabBarController);

                selectButton.TouchDown += (sender, e) =>
                {
                    CoinSelected(_coins[(int)picker.SelectedRowInComponent(0)]);
                    alert.Hide();
                };
                cancelButton.TouchDown += (sender, e) => { alert.Hide(); };

                popup.SizeToFit();
                Constants.CreateGradient(selectButton, 25);
            });
            pickerView.AddGestureRecognizer(pickerTap);
            View.AddSubview(pickerView);

            pickerView.AutoSetDimensionsToSize(new CGSize(125, 50));
            pickerView.AutoPinEdge(ALEdge.Left, ALEdge.Right, _amountTextField, 10);
            pickerView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            pickerView.AutoAlignAxis(ALAxis.Horizontal, _amountTextField);

            UIImageView pickerImage = new UIImageView(UIImage.FromBundle("ic_currency_picker.png"));
            pickerView.AddSubview(pickerImage);
            pickerImage.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            pickerImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            //_pickerLabel.Text = "STEEM";
            _pickerLabel.TextAlignment = UITextAlignment.Center;
            _pickerLabel.Font = Constants.Semibold14;
            _pickerLabel.TextColor = Constants.R255G71B5;
            pickerView.AddSubview(_pickerLabel);
            _pickerLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _pickerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 3);
            _pickerLabel.AutoPinEdge(ALEdge.Right, ALEdge.Left, pickerImage);

            _transferButton = new UIButton();
            _transferButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _transferButton.SetTitle("TRANSFER", UIControlState.Normal);
            _transferButton.Layer.CornerRadius = 25;
            _transferButton.Font = Constants.Bold14;
            _transferButton.TouchDown += Transfer;
            View.AddSubview(_transferButton);

            _transferButton.AutoSetDimension(ALDimension.Height, 50);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _transferButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _amountTextField, 25);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            Constants.CreateShadow(_transferButton, Constants.R204G204B204, 0.7f, 25, 10, 12);

            SetupTable();

            _noResultViewTags.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NoResultText);
            _noResultViewTags.Lines = 2;
            _noResultViewTags.Hidden = true;
            _noResultViewTags.TextAlignment = UITextAlignment.Center;
            _noResultViewTags.Font = Constants.Light27;
            _noResultViewTags.TextColor = Constants.R15G24B30;

            View.AddSubview(_noResultViewTags);
            _noResultViewTags.AutoPinEdge(ALEdge.Right, ALEdge.Right, _usersTable, -18);
            _noResultViewTags.AutoPinEdge(ALEdge.Left, ALEdge.Left, _usersTable, 18);
            _tagsNotFoundHorizontalAlignment = _noResultViewTags.AutoAlignAxis(ALAxis.Horizontal, _usersTable);

            warningView = new UIView();
            warningView.ClipsToBounds = true;
            warningView.BackgroundColor = Constants.R255G34B5;
            warningView.Alpha = 0;
            Constants.CreateShadow(warningView, Constants.R231G72B0, 0.5f, 6, 10, 12);
            View.AddSubview(warningView);

            warningView.AutoSetDimension(ALDimension.Height, 60);
            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            warningViewToBottomConstraint = warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var warningImage = new UIImageView();
            warningImage.Image = UIImage.FromBundle("ic_info");

            var warningLabel = new UILabel();
            warningLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TagSearchWarning);
            warningLabel.Lines = 3;
            warningLabel.Font = Constants.Regular12;
            warningLabel.TextColor = UIColor.FromRGB(255, 255, 255);

            warningView.AddSubview(warningLabel);
            warningView.AddSubview(warningImage);

            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            warningImage.AutoSetDimension(ALDimension.Width, 20);
            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);

            warningLabel.AutoPinEdge(ALEdge.Left, ALEdge.Right, warningImage, 20);
            warningLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

            var tap = new UITapGestureRecognizer(() =>
            {
                RemoveFocus();
            });
            View.AddGestureRecognizer(tap);
        }

        private void RemoveFocus()
        {
            _recepientTextField.ResignFirstResponder();
            _amountTextField.ResignFirstResponder();
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            var shift = -90;
            _tagsHorizontalAlignment.Constant = shift;
            _tagsNotFoundHorizontalAlignment.Constant = shift;
            warningView.Hidden = false;

            if (ScrollAmount == 0)
            {
                var r = UIKeyboard.FrameEndFromNotification(notification);
                ScrollAmount = r.Height;// add if iphone X     - 34;
                warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
            }

            ScrollTheView(true);
        }

        protected override void KeyBoardDownNotification(NSNotification notification)
        {
            warningView.Hidden = true;
            warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
            _tagsNotFoundHorizontalAlignment.Constant = 0;
            _tagsHorizontalAlignment.Constant = 0;
            ScrollTheView(false);
        }
    }

    public class CoinPickerViewModel : UIPickerViewModel
    {
        private readonly List<CurrencyType> _coins;
        //public Action<CurrencyType> CoinSelected;

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
            //CoinSelected?.Invoke(_coins[(int)row]);
            pickerView.ReloadComponent(0);
        }
    }
}
