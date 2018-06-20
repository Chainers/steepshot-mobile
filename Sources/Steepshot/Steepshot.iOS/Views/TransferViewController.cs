using System;
using System.Collections.Generic;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Errors;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class TransferViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        private UIButton _transferButton = new UIButton();
        private UILabel _pickerLabel = new UILabel();
        private SearchTextField _recepientTextField;
        private SearchTextField _amountTextField;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateView();
            SetBackButton();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(_transferButton, 25);
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

            var response = await _presenter.TryTransfer(_recepientTextField.Text, double.Parse(_amountTextField.Text), Core.Models.Requests.CurrencyType.Steem);
            if (response.IsSuccess)
            {
                return;
            }
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = "Transfer";

            //Create view with balance
            //NavigationItem.RightBarButtonItem
        }

        private async void LoadUsers()
        {
            var error = await _presenter.TryLoadNextSearchUser(_recepientTextField.Text);
            if (error is CanceledError)
                return;
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

            _recepientTextField = new SearchTextField(() => {

            }, "Search");
            _recepientTextField.Layer.CornerRadius = 25;
            View.AddSubview(_recepientTextField);

            _recepientTextField.ClearButtonTapped += () => { };

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

            _amountTextField = new SearchTextField(() => {

            }, "100");
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
            var pickerTap = new UITapGestureRecognizer();
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

            _pickerLabel.Text = "STEEM";
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

            //Pop up for picker

            UIView popup = new UIView();
            View.AddSubview(popup);
            popup.AutoSetDimensionsToSize(new CGSize(300, 500));

            UIPickerView picker = new UIPickerView();
            popup.AddSubview(picker);

            picker.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _transferButton, 10);
            picker.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            picker.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            picker.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            picker.Model = new StatusPickerViewModel();
        }
    }

    public class StatusPickerViewModel : UIPickerViewModel
    {
        List<string> gf = new List<string>();

        public StatusPickerViewModel()
        {
            for (int i = 0; i < 10; i++)
            {
                gf.Add(i.ToString());
            }
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return 5;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return gf[(int)row];
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }
    }
}
