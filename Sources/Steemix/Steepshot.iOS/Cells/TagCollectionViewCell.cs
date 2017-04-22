using System;

using Foundation;
using UIKit;

namespace Steepshot.iOS
{
    public partial class TagCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("TagCollectionViewCell");
        public static readonly UINib Nib;
        private bool isButtonSetted;

        public string TagText
        {
            set{ tagText.Text = value; }
        }

        static TagCollectionViewCell()
        {
            Nib = UINib.FromName("TagCollectionViewCell", NSBundle.MainBundle);
        }

        protected TagCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public void SetButton(EventHandler ButtonAction)
        {
            addTagsButton.Hidden = false;
            closeImage.Hidden = true;
            if (!isButtonSetted)
            {
                addTagsButton.TouchDown += ButtonAction;
                isButtonSetted = true;
            }
        }

        public void RefreshCell()
        {
            addTagsButton.Hidden = true;
            closeImage.Hidden = false;
        }
    }
}
