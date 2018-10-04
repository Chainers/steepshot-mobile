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
        private readonly UIButton _buttonOverlay = new UIButton();

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

                addImage.Hidden = HidePlus;

                _buttonOverlay.BackgroundColor = UIColor.Clear;
                ContentView.AddSubview(_buttonOverlay);

                _buttonOverlay.TouchDown += ButtonOverlay_TouchDown;
                _buttonOverlay.TouchCancel += ButtonOverlay_TouchCancel;
                _buttonOverlay.TouchUpInside += TouchUp;
                _buttonOverlay.TouchUpOutside += TouchUp;
                _buttonOverlay.AutoPinEdgesToSuperviewEdges();
                _isInitialized = true;
            }
            base.LayoutSubviews();
        }

        private void ButtonOverlay_TouchCancel(object sender, EventArgs e)
        {
            addImage.Image = UIImage.FromBundle("ic_add_tag.png");
        }

        private void ButtonOverlay_TouchDown(object sender, EventArgs e)
        {
            addImage.Image = UIImage.FromBundle("ic_add_tag_active.png");
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

        public void RemoveEvents()
        {
            _buttonOverlay.TouchDown -= ButtonOverlay_TouchDown;
            _buttonOverlay.TouchCancel -= ButtonOverlay_TouchCancel;
            _buttonOverlay.TouchUpInside -= TouchUp;
            _buttonOverlay.TouchUpOutside -= TouchUp;
        }
    }
}
