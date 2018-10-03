using System;
using Foundation;
using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Enums;
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

        private UITapGestureRecognizer _tap;
        UITapGestureRecognizer _deleteTap;

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
                _tap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Tap, _currentAccount);
                });
                _deleteTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Delete, _currentAccount);
                });

                closeButton.AddGestureRecognizer(_deleteTap);
                ContentView.AddGestureRecognizer(_tap);

                networkName.Font = Helpers.Constants.Semibold14;

                _isInitialized = true;
            }
        }

        public void UpdateCell(UserInfo user)
        {
            _currentAccount = user;
            networkName.Text = $"{_currentAccount.Chain.ToString()} account";
            networkStatus.Image = AppDelegate.MainChain == _currentAccount.Chain ? UIImage.FromBundle("ic_activated") : UIImage.FromBundle("ic_deactivated");
        }

        public void ReleaseCell()
        {
            closeButton.RemoveGestureRecognizer(_deleteTap);
            ContentView.RemoveGestureRecognizer(_tap);
        }
    }
}
