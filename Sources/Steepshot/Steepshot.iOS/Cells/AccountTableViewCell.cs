using System;

using Foundation;
using Steepshot.Core.Authority;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class AccountTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("AccountTableViewCell");
        public static readonly UINib Nib;
        public bool IsCellActionSet => CellAction != null;
        public Action<ActionType, UserInfo> CellAction;
        private bool _isInitialized;
        private UserInfo _currentAccount;

        static AccountTableViewCell()
        {
            Nib = UINib.FromName("AccountTableViewCell", NSBundle.MainBundle);
        }

        protected AccountTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void LayoutSubviews()
        {
            if (!_isInitialized)
            {
                var tap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Tap, _currentAccount);
                });
                var deleteTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Delete, _currentAccount);
                });

                closeButton.AddGestureRecognizer(deleteTap);
                ContentView.AddGestureRecognizer(tap);

                networkName.Font = Helpers.Constants.Semibold14;

                _isInitialized = true;
            }
        }

        public void UpdateCell(UserInfo user)
        {
            _currentAccount = user;
            networkName.Text = $"{_currentAccount.Chain.ToString()} account";
            networkStatus.Image = BasePresenter.Chain == _currentAccount.Chain ?
                UIImage.FromBundle("ic_activated") :  UIImage.FromBundle("ic_deactivated");
        }
    }
}
