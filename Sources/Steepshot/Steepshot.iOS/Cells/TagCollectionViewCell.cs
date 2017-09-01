using System;
using Foundation;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class TagCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString(nameof(TagCollectionViewCell));
        public static readonly UINib Nib;
        private bool _isButtonSetted;

        public string TagText
        {
            set { tagText.Text = value; }
        }

        static TagCollectionViewCell()
        {
            Nib = UINib.FromName(nameof(TagCollectionViewCell), NSBundle.MainBundle);
        }

        protected TagCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public void SetButton(EventHandler buttonAction)
        {
            addTagsButton.Hidden = false;
            closeImage.Hidden = true;
            if (!_isButtonSetted)
            {
                addTagsButton.TouchDown += buttonAction;
                _isButtonSetted = true;
            }
        }

        public void RefreshCell()
        {
            addTagsButton.Hidden = true;
            closeImage.Hidden = false;
        }
    }
}