using System;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Models.Enums;
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
        public bool HidePlus;

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
                tagLabel.Font = Helpers.Constants.Semibold14;

                var buttonOverlay = new UIButton();
                buttonOverlay.BackgroundColor = UIColor.Clear;
                ContentView.AddSubview(buttonOverlay);

                buttonOverlay.TouchDown += (object sender, EventArgs e) =>
                {
                    addImage.Image = UIImage.FromBundle("ic_add_tag_active.png");
                };

                buttonOverlay.TouchCancel += (object sender, EventArgs e) =>
                {
                    addImage.Image = UIImage.FromBundle("ic_add_tag.png");
                };

                addImage.Hidden = HidePlus;

                buttonOverlay.TouchUpInside += TouchUp;
                buttonOverlay.TouchUpOutside += TouchUp;

                buttonOverlay.AutoPinEdgesToSuperviewEdges();
                _isInitialized = true;
            }
            base.LayoutSubviews();
        }

        private void TouchUp(object sender, EventArgs e)
        {
            addImage.Image = UIImage.FromBundle("ic_add_tag.png");
            CellAction?.Invoke(ActionType.Tap, tagLabel.Text);
        }

        public void UpdateCell(string tag)
        {
            addImage.Image = UIImage.FromBundle("ic_add_tag.png");
            tagLabel.Text = tag;
        }
    }
}
