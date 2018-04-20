using System;
using Foundation;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class LocalTagCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("LocalTagCollectionViewCell");
        public static readonly UINib Nib;

        private bool _isInitialized;
        public Action<ActionType, string> CellAction;
        public bool IsCellActionSet => CellAction != null;

        static LocalTagCollectionViewCell()
        {
            Nib = UINib.FromName("LocalTagCollectionViewCell", NSBundle.MainBundle);
        }

        protected LocalTagCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void LayoutSubviews()
        {
            if (!_isInitialized)
            {
                var tap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Tap, tagText.Text);
                });

                tagText.Font = Constants.Regular14;
                rootView.Layer.CornerRadius = 20;
                rootView.AddGestureRecognizer(tap);

                _isInitialized = true;
            }
            base.LayoutSubviews();
        }

        public void RefreshCell(string cellText)
        {
            tagText.Text = cellText;
            LayoutIfNeeded();
        }
    }
}
