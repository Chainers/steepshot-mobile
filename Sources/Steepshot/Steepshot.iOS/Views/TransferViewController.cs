using System;
using PureLayout.Net;
using Steepshot.Core.Presenters;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class TransferViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            UILabel recepientLabel = new UILabel();
            recepientLabel.Text = "Recipient name";
            View.AddSubview(recepientLabel);

            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 25);
            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);

            UITextField recepientTextField = new UITextField();
            recepientTextField.Placeholder = "login";
            View.AddSubview(recepientTextField);

            recepientTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            recepientTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recepientLabel, 25);
            recepientTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            recepientTextField.AutoSetDimension(ALDimension.Height, 50);

            UILabel amountLabel = new UILabel();
            amountLabel.Text = "Transfer amount";
            View.AddSubview(amountLabel);

            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            amountLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recepientTextField, 25);
            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);

            UITextField amountTextField = new UITextField();
            amountTextField.Placeholder = "amount";
            View.AddSubview(amountTextField);

            amountTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            amountTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountLabel, 25);
            amountTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            amountTextField.AutoSetDimension(ALDimension.Height, 50);
            /*
            UIView pickerView = new UIView();
            pickerView.BackgroundColor = UIColor.Black;
            View.AddSubview(pickerView);
*/
            UIButton _transferButton = new UIButton();
            _transferButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _transferButton.SetTitle("Transfer", UIControlState.Normal);
            _transferButton.TouchDown += (object sender, EventArgs e) =>
            {

            };
            View.AddSubview(_transferButton);

            _transferButton.AutoSetDimension(ALDimension.Height, 50);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _transferButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountTextField, 25);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
        }

        private void CreateView()
        {
            
        }
    }
}
