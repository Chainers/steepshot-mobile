using System;
using Foundation;
using Steepshot.Core.Models;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class TagTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("TagTableViewCell");
        public static readonly UINib Nib;

        private bool _isInitialized;
        public Action<ActionType, string> CellAction;
        public bool IsCellActionSet => CellAction != null;

        static TagTableViewCell()
        {
            Nib = UINib.FromName("TagTableViewCell", NSBundle.MainBundle);
        }

        protected TagTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void LayoutSubviews()
        {
            if (!_isInitialized)
            {
                SelectionStyle = UITableViewCellSelectionStyle.None;
                var tap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Tap, tagLabel.Text);
                });
                ContentView.AddGestureRecognizer(tap);
                tagLabel.Font = Helpers.Constants.Semibold14;
                hashLabel.Font = Helpers.Constants.Semibold14;

                _isInitialized = true;
            }

            base.LayoutSubviews();
        }

        public void UpdateCell(string tag)
        {
            tagLabel.Text = tag;
        }
    }
}
