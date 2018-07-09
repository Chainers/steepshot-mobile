using System;
using PureLayout.Net;
using Steepshot.Core.Presenters;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class WalletViewController : BaseViewControllerWithPresenter<WalletPresenter>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            var _sendButton = new UIButton();
            _sendButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _sendButton.SetTitle("Send money", UIControlState.Normal);
            _sendButton.TouchDown+= (object sender, EventArgs e) => 
            {
                NavigationController.PushViewController(new TransferViewController(), true);
            };

            View.Add(_sendButton);

            _sendButton.AutoCenterInSuperview();
        }
    }
}
