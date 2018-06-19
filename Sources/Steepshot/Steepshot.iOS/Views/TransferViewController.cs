using System;
using PureLayout.Net;
using Steepshot.Core.Presenters;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class TransferViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        UIButton _transferButton = new UIButton();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            UILabel recepientLabel = new UILabel();
            recepientLabel.Text = "Recipient name";
            recepientLabel.Font = Constants.Semibold14;
            recepientLabel.TextColor = UIColor.Black;
            View.AddSubview(recepientLabel);

            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 25);
            recepientLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            /*
            UITextField recepientTextField = new UITextField();
            recepientTextField.Placeholder = "Search";
            recepientTextField.Layer.CornerRadius = 25;
            recepientTextField.BackgroundColor = Constants.R245G245B245;
            recepientTextField.RightView = new UIImageView() { Image = UIImage.FromBundle("ic_search_small.png") };
            recepientTextField.Font = Constants.Regular14;
*/
            SearchTextField recepientTextField = new SearchTextField(() => {
                
            }, "Tap to search");
            recepientTextField.BecomeFirstResponder();
            recepientTextField.Font = Constants.Regular14;
            recepientTextField.Placeholder = "Search";
            recepientTextField.Layer.CornerRadius = 25;
            View.AddSubview(recepientTextField);

            recepientTextField.ClearButtonTapped += () => { };

            View.AddSubview(recepientTextField);

            recepientTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            recepientTextField.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recepientLabel, 16);
            recepientTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            recepientTextField.AutoSetDimension(ALDimension.Height, 50);

            UILabel amountLabel = new UILabel();
            amountLabel.Text = "Transfer amount";
            amountLabel.Font = Constants.Semibold14;
            amountLabel.TextColor = UIColor.Black;
            View.AddSubview(amountLabel);

            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            amountLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, recepientTextField, 25);
            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);

            UITextField amountTextField = new UITextField();
            amountTextField.Placeholder = "100";
            amountTextField.Layer.CornerRadius = 25;
            amountTextField.BackgroundColor = Constants.R245G245B245;
            recepientTextField.Font = Constants.Regular14;
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
            _transferButton = new UIButton();
            _transferButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _transferButton.SetTitle("Transfer", UIControlState.Normal);
            _transferButton.Layer.CornerRadius = 25;
            _transferButton.TouchDown += (object sender, EventArgs e) =>
            {
                
            };
            View.AddSubview(_transferButton);

            _transferButton.AutoSetDimension(ALDimension.Height, 50);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _transferButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountTextField, 25);
            _transferButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            Constants.CreateShadow(_transferButton, Constants.R204G204B204, 0.7f, 25, 10, 12);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(_transferButton, 25);
        }

        private void CreateView()
        {
            
        }
    }
}
