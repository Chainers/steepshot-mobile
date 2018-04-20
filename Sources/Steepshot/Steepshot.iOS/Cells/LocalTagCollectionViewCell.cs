using System;
using Foundation;
using PureLayout.Net;
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
                tagText.Font = Constants.Regular14;
                rootView.Layer.CornerRadius = 20;

                var buttonOverlay = new UIButton();
                buttonOverlay.BackgroundColor = UIColor.Clear;
                ContentView.AddSubview(buttonOverlay);

                buttonOverlay.TouchDown += (object sender, EventArgs e) =>
                {
                    closeImage.Image = UIImage.FromBundle("ic_delete_tag.png");
                };

                buttonOverlay.TouchCancel += (object sender, EventArgs e) =>
                {
                    closeImage.Image = UIImage.FromBundle("ic_close_tag.png");
                };

                buttonOverlay.TouchUpInside += TouchUp;
                buttonOverlay.TouchUpOutside += TouchUp;

                buttonOverlay.AutoPinEdgesToSuperviewEdges();

                _isInitialized = true;
            }
            base.LayoutSubviews();
        }

        private void TouchUp(object sender, EventArgs e)
        {
            closeImage.Image = UIImage.FromBundle("ic_close_tag.png");
            CellAction?.Invoke(ActionType.Tap, tagText.Text);
        }

        public void RefreshCell(string cellText)
        {
            tagText.Text = cellText;
            LayoutIfNeeded();
        }
    }
}
