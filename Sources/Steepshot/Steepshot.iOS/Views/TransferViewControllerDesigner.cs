using System;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Constants = Steepshot.iOS.Helpers.Constants;
using UIKit;
using Steepshot.iOS.ViewSources;
using Steepshot.iOS.Cells;
using Steepshot.Core.Localization;
using Steepshot.iOS.Helpers;
using System.Collections.Generic;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Extensions;

namespace Steepshot.iOS.Views
{
    public partial class TransferViewController
    {
        private UIButton _transferButton = new UIButton();
        private UILabel _pickerLabel = new UILabel();
        private SearchTextField _recepientTextField;
        private SearchTextField _amountTextField;
        private UITextView _memoTextView;
        private UITableView _usersTable;
        private FollowTableViewSource _userTableSource;
        private UIActivityIndicatorView _usersLoader;
        private NSLayoutConstraint _userLoaderHorizontalAlignment;
        private NSLayoutConstraint _usersNotFoundHorizontalAlignment;
        private NSLayoutConstraint warningViewToBottomConstraint;
        private UIView warningView;
        private UILabel _noResultViewTags = new UILabel();
        private UIActivityIndicatorView _balanceLoader = new UIActivityIndicatorView();
        private UILabel _balanceLabel;
        private UIImageView _recipientAvatar;
        private BaseTextViewDelegate _memoTextViewDelegate;
        private nfloat _tableScrollAmount;
        private bool _isWarningOpen;
        private CustomAlertView _alert;
        private UIActivityIndicatorView _tranfserLoader;
        private UILabel memoLabel;

        private CustomAlertView _successAlert;
        private UILabel recipientValue;
        private UILabel amountValue;
        private UILabel errorMessage = new UILabel();

        private void SetupTable()
        {
            _usersTable = new UITableView();
            _usersTable.Hidden = true;
            View.AddSubview(_usersTable);

            _usersTable.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _recepientTextField, 25);
            _usersTable.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _usersTable.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _usersTable.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _userTableSource = new FollowTableViewSource(_transferFacade.UserFriendPresenter, _usersTable, true);
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

            _userLoaderHorizontalAlignment = _usersLoader.AutoAlignAxis(ALAxis.Horizontal, _usersTable);
            _usersLoader.AutoAlignAxis(ALAxis.Vertical, _usersTable);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.Transfer);

            var username = new UILabel(new CGRect(0, 0, 100, 50))
            {
                Font = Constants.Semibold14,
                TextColor = Constants.R255G34B5,
                TextAlignment = UITextAlignment.Right,
                Text = $"@{AppSettings.User.Login}"
            };

            var rightBarButton = new UIBarButtonItem(username);
            NavigationItem.RightBarButtonItem = rightBarButton;
        }

        private void CreateView()
        {
            View.BackgroundColor = UIColor.White;

            UILabel recepientLabel = new UILabel();
            recepientLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.RecipientName);
            recepientLabel.Font = Constants.Semibold14;
            recepientLabel.TextColor = UIColor.Black;
            View.AddSubview(recepientLabel);

            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 25);
            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);

            var recipientStackView = new UIStackView();
            recipientStackView.Spacing = 10;
            View.AddSubview(recipientStackView);

            recipientStackView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            recipientStackView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recepientLabel, 16);
            recipientStackView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            recipientStackView.AutoSetDimension(ALDimension.Height, 50);

            _recipientAvatar = new UIImageView();
            _recipientAvatar.Layer.CornerRadius = 25;
            _recipientAvatar.ClipsToBounds = true;
            _recipientAvatar.Hidden = true;
            recipientStackView.AddArrangedSubview(_recipientAvatar);
            _recipientAvatar.AutoSetDimension(ALDimension.Width, 50);

            _recepientTextField = new SearchTextField(AppSettings.LocalizationManager.GetText(LocalizationKeys.RecipientNameHint));

            _recepientTextField.ReturnButtonTapped += () => {
                RemoveFocus();
            };

            _recepientTextField.EditingChanged += EditingChanged;
            _recepientTextField.Layer.CornerRadius = 25;
            _recepientTextField.EditingChanged += (object sender, EventArgs e) =>
            {
                _transferFacade.Recipient = null;
            };

            recipientStackView.AddArrangedSubview(_recepientTextField);

            _recepientTextField.EditingDidBegin += (object sender, EventArgs e) =>
            {
                _usersTable.Hidden = false;
            };

            _recepientTextField.EditingDidEnd += (object sender, EventArgs e) =>
            {
                _usersTable.Hidden = true;
                _usersLoader.StopAnimating();
                _noResultViewTags.Hidden = true;
            };

            _recepientTextField.ClearButtonTapped += () =>
            {
                UIView.Animate(0.2, () =>
                {
                    _recipientAvatar.Hidden = true;
                    _recipientAvatar.LayoutIfNeeded();
                });

                _prevQuery = string.Empty;
                _transferFacade.Recipient = null;
                _transferFacade.UserFriendPresenter.Clear();
                _recepientTextField.Text = _transferFacade?.Recipient?.Author;
            };

            UILabel amountLabel = new UILabel();
            amountLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmount);
            amountLabel.Font = Constants.Semibold14;
            amountLabel.TextColor = UIColor.Black;
            View.AddSubview(amountLabel);

            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            amountLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recipientStackView, 25);

            _balanceLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            _balanceLoader.Color = UIColor.Black;
            _balanceLoader.HidesWhenStopped = true;
            _balanceLoader.StartAnimating();

            View.AddSubview(_balanceLoader);

            _balanceLoader.AutoPinEdge(ALEdge.Left, ALEdge.Right, amountLabel, 10);
            _balanceLoader.AutoAlignAxis(ALAxis.Horizontal, amountLabel);

            _balanceLabel = new UILabel();
            _balanceLabel.Font = Constants.Semibold14;
            _balanceLabel.TextColor = Constants.R151G155B158;
            _balanceLabel.TextAlignment = UITextAlignment.Right;

            View.AddSubview(_balanceLabel);

            _balanceLabel.AutoAlignAxis(ALAxis.Horizontal, amountLabel);
            _balanceLabel.AutoPinEdge(ALEdge.Left, ALEdge.Right, amountLabel, 5);
            _balanceLabel.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            var rightView = new UIView();
            View.AddSubview(rightView);
            rightView.AutoSetDimension(ALDimension.Height, 50);

            UIImageView pickerImage = new UIImageView(UIImage.FromBundle("ic_currency_picker.png"));
            rightView.AddSubview(pickerImage);
            pickerImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            pickerImage.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);

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
            View.AddSubview(_amountTextField);

            //_amountTextField.ClearButtonTapped += () => { };
            _amountTextField.EditingChanged += IsEnoughBalance;

            View.AddSubview(_amountTextField);

            _amountTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _amountTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountLabel, 16);
            _amountTextField.AutoSetDimension(ALDimension.Height, 50);

            var max = new UIButton();
            max.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Max), UIControlState.Normal);
            max.SetTitleColor(UIColor.Black, UIControlState.Normal);
            max.Font = Constants.Semibold14;
            max.Layer.BorderWidth = 1;
            max.Layer.BorderColor = Constants.R245G245B245.CGColor;
            max.Layer.CornerRadius = 25;

            View.AddSubview(max);

            max.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            max.AutoPinEdge(ALEdge.Left, ALEdge.Right, _amountTextField, 10);
            max.AutoSetDimensionsToSize(new CGSize(80, 50));
            max.AutoAlignAxis(ALAxis.Horizontal, _amountTextField);
            max.TouchDown += MaxBtnOnClick;

            rightView.AutoAlignAxis(ALAxis.Horizontal, _amountTextField);
            rightView.AutoPinEdge(ALEdge.Right, ALEdge.Right, _amountTextField);
            View.BringSubviewToFront(rightView);
            _balanceLabel.AutoPinEdge(ALEdge.Right, ALEdge.Right, _amountTextField);

            UIView pickerView = new UIView();
            pickerView.Layer.CornerRadius = 25;
            pickerView.Layer.BorderColor = Constants.R244G244B246.CGColor;
            pickerView.Layer.BorderWidth = 1;
            pickerView.UserInteractionEnabled = true;
            var pickerTap = new UITapGestureRecognizer(() =>
            {
                if (_alert == null)
                {
                    var popup = new UIView();
                    popup.ClipsToBounds = true;
                    popup.Layer.CornerRadius = 15;
                    popup.BackgroundColor = UIColor.White;
                    View.AddSubview(popup);
                    var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
                    popup.AutoSetDimension(ALDimension.Width, dialogWidth);

                    var commonMargin = 20;

                    var title = new UILabel();
                    title.Font = Constants.Semibold14;
                    title.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SelectToken);
                    title.SizeToFit();
                    popup.AddSubview(title);
                    title.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
                    title.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
                    title.AutoSetDimension(ALDimension.Height, 70);

                    UIPickerView picker = new UIPickerView();
                    picker.Select(_coins.IndexOf(_pickedCoin), 0, true);
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

                    var separator = new UIView();
                    separator.BackgroundColor = Constants.R245G245B245;
                    popup.AddSubview(separator);

                    separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, picker);
                    separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                    separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                    separator.AutoSetDimension(ALDimension.Height, 1);

                    var selectButton = new UIButton();
                    selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Select), UIControlState.Normal);
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

                    _alert = new CustomAlertView(popup, NavigationController);

                    selectButton.TouchDown += (sender, e) =>
                    {
                        CoinSelected(_coins[(int)picker.SelectedRowInComponent(0)]);
                        _alert.Close();
                        _amountTextField.UpdateRightViewRect();
                    };
                    cancelButton.TouchDown += (sender, e) => { _alert.Close(); };

                    popup.SizeToFit();
                    Constants.CreateGradient(selectButton, 25);
                }
                _alert.Show();
            });
            rightView.AddGestureRecognizer(pickerTap);

            var bottomStackView = new UIStackView();
            bottomStackView.Axis = UILayoutConstraintAxis.Vertical;
            View.AddSubview(bottomStackView);

            bottomStackView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            bottomStackView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _amountTextField, 5);
            bottomStackView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            bottomStackView.Spacing = 16;

            errorMessage.Font = Constants.Semibold12;
            errorMessage.Hidden = true;
            errorMessage.TextColor = Constants.R255G34B5;
            errorMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AmountLimitFull);
            bottomStackView.AddArrangedSubview(errorMessage);

            memoLabel = new UILabel();
            memoLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferComment);
            memoLabel.TextColor = Constants.R255G71B5;
            memoLabel.Font = Constants.Semibold14;
            memoLabel.UserInteractionEnabled = true;

            var memoTap = new UITapGestureRecognizer(() =>
            {
                memoLabel.TextColor = UIColor.Black;
                memoLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferComment);
                UIView.Animate(0.2, () =>
                {
                    _memoTextView.Hidden = false;
                    _memoTextView.LayoutIfNeeded();
                });
            });

            memoLabel.AddGestureRecognizer(memoTap);
            bottomStackView.AddArrangedSubview(memoLabel);

            memoLabel.AutoSetDimension(ALDimension.Height, 30);

            _memoTextView = new UITextView();
            _memoTextView.Layer.CornerRadius = 25;
            _memoTextView.TextContainerInset = new UIEdgeInsets(10, 15, 10, 15);
            _memoTextView.Font = Constants.Regular14;
            _memoTextView.Bounces = false;
            _memoTextView.ShowsVerticalScrollIndicator = false;
            _memoTextView.Hidden = true;
            _memoTextView.TintColor = Constants.R255G71B5;
            _memoTextView.BackgroundColor = Constants.R245G245B245;
            _memoTextViewDelegate = new BaseTextViewDelegate();
            _memoTextView.Delegate = _memoTextViewDelegate;

            _memoTextViewDelegate.EditingEndedAction += () =>
            {
                base.ScrollTheView(false);
            };

            bottomStackView.AddArrangedSubview(_memoTextView);

            _memoTextView.AutoSetDimension(ALDimension.Height, 80);
            var buttonWrapper = new UIView();

            _transferButton = new UIButton();
            _transferButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _transferButton.SetTitleColor(UIColor.Clear, UIControlState.Disabled);
            _transferButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Transfer).ToUpper(), UIControlState.Normal);
            _transferButton.Layer.CornerRadius = 25;
            _transferButton.Font = Constants.Bold14;
            _transferButton.TouchDown += Transfer;
            buttonWrapper.AddSubview(_transferButton);

            _transferButton.AutoSetDimension(ALDimension.Height, 50);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            Constants.CreateShadow(_transferButton, Constants.R204G204B204, 0.7f, 25, 10, 12);

            _tranfserLoader = new UIActivityIndicatorView();
            _tranfserLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            _tranfserLoader.HidesWhenStopped = true;
            buttonWrapper.AddSubview(_tranfserLoader);

            _tranfserLoader.AutoAlignAxis(ALAxis.Horizontal, _transferButton);
            _tranfserLoader.AutoAlignAxis(ALAxis.Vertical, _transferButton);

            bottomStackView.AddArrangedSubview(buttonWrapper);

            buttonWrapper.AutoSetDimension(ALDimension.Height, 65);

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
            _usersNotFoundHorizontalAlignment = _noResultViewTags.AutoAlignAxis(ALAxis.Horizontal, _usersTable);

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

        private void MaxBtnOnClick(object sender, EventArgs e)
        {
            _amountTextField.Text = _transferFacade.UserBalance.Value.ToBalanceValueString();
            IsEnoughBalance(null, null);
        }

        private void IsEnoughBalance(object sender, EventArgs e)
        {
            errorMessage.Hidden = true;
            var transferAmount = _amountTextField.GetDoubleValue();
            if (transferAmount == 0)
                return;

            if (transferAmount > _transferFacade.UserBalance.Value)
            {
                errorMessage.Hidden = false;
            }
        }

        private void ShowSuccessPopUp()
        {
            if (_successAlert == null)
            {
                var popup = new UIView();
                popup.ClipsToBounds = true;
                popup.Layer.CornerRadius = 15;
                popup.BackgroundColor = UIColor.White;
                View.AddSubview(popup);
                var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
                popup.AutoSetDimension(ALDimension.Width, dialogWidth);

                var commonMargin = 20;
                var image = new UIImageView();
                image.Image = UIImage.FromBundle("ic_stamp");
                image.ContentMode = UIViewContentMode.Center;
                popup.AddSubview(image);

                image.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
                image.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 30);

                var label = new UILabel();
                label.Text = "Transaction completed successfully!";
                label.Font = DeviceHelper.IsSmallDevice ? Constants.Light23 : Constants.Light27;
                label.Lines = 2;
                label.TextAlignment = UITextAlignment.Center;

                popup.AddSubview(label);

                label.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, image, 30);
                label.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                label.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);

                var recipientView = new UIView();
                recipientView.BackgroundColor = Constants.R250G250B250;
                recipientView.Layer.CornerRadius = 10;
                recipientView.ClipsToBounds = true;
                popup.AddSubview(recipientView);

                recipientView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, label, 37);
                recipientView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                recipientView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                recipientView.AutoSetDimension(ALDimension.Height, 50);

                var recipientLabel = new UILabel();
                recipientLabel.Text = "Recipient";
                recipientLabel.Font = Constants.Regular14;
                recipientView.AddSubview(recipientLabel);

                recipientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
                recipientLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
                recipientLabel.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);

                recipientValue = new UILabel();
                recipientValue.Font = Constants.Semibold14;
                recipientValue.TextColor = Constants.R255G34B5;
                recipientValue.TextAlignment = UITextAlignment.Right;
                recipientView.AddSubview(recipientValue);

                recipientValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
                recipientValue.AutoPinEdge(ALEdge.Left, ALEdge.Right, recipientLabel, 20);
                recipientValue.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

                var amountView = new UIView();
                amountView.BackgroundColor = Constants.R250G250B250;
                amountView.Layer.CornerRadius = 10;
                amountView.ClipsToBounds = true;
                popup.AddSubview(amountView);

                amountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recipientView, 10);
                amountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                amountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                amountView.AutoSetDimension(ALDimension.Height, 50);

                var amountLabel = new UILabel();
                amountLabel.Text = "Amount";
                amountLabel.Font = Constants.Regular14;
                amountView.AddSubview(amountLabel);

                amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
                amountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
                amountLabel.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);

                amountValue = new UILabel();
                amountValue.Font = Constants.Semibold14;
                amountValue.TextColor = Constants.R255G34B5;
                amountValue.TextAlignment = UITextAlignment.Right;
                amountView.AddSubview(amountValue);

                amountValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
                amountValue.AutoPinEdge(ALEdge.Left, ALEdge.Right, amountLabel, 20);
                amountValue.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

                var separator = new UIView();
                separator.BackgroundColor = Constants.R245G245B245;
                popup.AddSubview(separator);

                separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountView, 20);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                separator.AutoSetDimension(ALDimension.Height, 1);

                var cancelButton = new UIButton();
                cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Close), UIControlState.Normal);
                cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
                cancelButton.Layer.CornerRadius = 25;
                cancelButton.Font = Constants.Semibold14;
                cancelButton.Layer.BorderWidth = 1;
                cancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
                popup.AddSubview(cancelButton);

                cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
                cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
                cancelButton.AutoSetDimension(ALDimension.Height, 50);

                NavigationController.View.EndEditing(true);

                _successAlert = new CustomAlertView(popup, NavigationController);
                cancelButton.TouchDown += (sender, e) => { _successAlert.Close(); };

                popup.SizeToFit();
            }

            recipientValue.Text = _transferFacade.Recipient.Author;
            amountValue.Text = _amountTextField.Text;

            _successAlert.Show();
        }

        private void SetPlaceholder()
        {
            var placeholderLabel = new UILabel();
            placeholderLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PutYourComment);
            placeholderLabel.SizeToFit();
            placeholderLabel.Font = Constants.Regular14;
            placeholderLabel.TextColor = Constants.R151G155B158;
            placeholderLabel.Hidden = false;

            var labelX = _memoTextView.TextContainerInset.Left;
            var labelY = _memoTextView.TextContainerInset.Top;
            var labelWidth = placeholderLabel.Frame.Width;
            var labelHeight = placeholderLabel.Frame.Height;

            placeholderLabel.Frame = new CGRect(labelX, labelY, labelWidth, labelHeight);

            _memoTextView.AddSubview(placeholderLabel);

            _memoTextViewDelegate.Placeholder = placeholderLabel;
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
            pickerLabel.Text = _coins[(int)row].ToString().ToUpper();
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
